//! The bridge to the engine sidecar.
//!
//! One child process speaks newline-delimited JSON on stdin/stdout (see
//! `docs/decisions/0002`). This module owns that process: it correlates
//! responses to requests by id, bounds every request with a timeout, and
//! respawns the engine on the next call if it has died.

use std::collections::HashMap;
use std::path::PathBuf;
use std::process::Stdio;
use std::sync::atomic::{AtomicU64, Ordering};
use std::sync::{Arc, Mutex as SyncMutex};
use std::time::Duration;

use serde::Serialize;
use serde_json::{json, Value};
use tokio::io::{AsyncBufReadExt, AsyncWriteExt, BufReader};
use tokio::process::{Child, ChildStdin, ChildStdout, Command};
use tokio::sync::{oneshot, Mutex};

/// Longest a single request may take. Whole-book work takes well under a
/// second, so hitting this means the engine is stuck, and it is restarted.
const REQUEST_TIMEOUT: Duration = Duration::from_secs(30);

const MAX_METHOD_LENGTH: usize = 64;

/// An error the UI can show and branch on. Engine errors pass through with
/// their own code; bridge failures use the codes defined here.
#[derive(Debug, Clone, Serialize, PartialEq, Eq)]
pub struct EngineError {
    pub code: String,
    pub message: String,
}

impl EngineError {
    pub(crate) fn new(code: &str, message: impl Into<String>) -> Self {
        Self {
            code: code.to_owned(),
            message: message.into(),
        }
    }

    fn unavailable(detail: impl std::fmt::Display) -> Self {
        Self::new(
            "engine_unavailable",
            format!("The analysis engine could not be started: {detail}"),
        )
    }

    fn stopped() -> Self {
        Self::new(
            "engine_stopped",
            "The analysis engine stopped unexpectedly. It restarts on the next request.",
        )
    }
}

type Reply = Result<Value, EngineError>;
type Pending = Arc<SyncMutex<HashMap<u64, oneshot::Sender<Reply>>>>;

/// A live engine process and the requests waiting on it. Each process has its
/// own pending map, so a process that dies can only fail its own requests,
/// never those of the process that replaced it.
struct Running {
    child: Child,
    stdin: ChildStdin,
    pending: Pending,
}

pub struct Engine {
    binary: PathBuf,
    content: PathBuf,
    user: Option<PathBuf>,
    next_id: AtomicU64,
    running: Mutex<Option<Running>>,
}

impl Engine {
    /// `user` is the reader's writable data file; without it, bookmarks and
    /// history report that they are unavailable.
    pub fn new(binary: PathBuf, content: PathBuf, user: Option<PathBuf>) -> Self {
        Self {
            binary,
            content,
            user,
            next_id: AtomicU64::new(1),
            running: Mutex::new(None),
        }
    }

    /// Sends one request and waits for its response.
    pub async fn call(&self, method: &str, params: Option<Value>) -> Reply {
        validate_method(method)?;

        let id = self.next_id.fetch_add(1, Ordering::Relaxed);
        let line = format!(
            "{}\n",
            json!({ "id": id, "method": method, "params": params })
        );
        let (sender, receiver) = oneshot::channel();

        let pending = {
            let mut guard = self.running.lock().await;
            let running = self.ensure_running(&mut guard)?;
            lock(&running.pending).insert(id, sender);

            if let Err(error) = running.stdin.write_all(line.as_bytes()).await {
                lock(&running.pending).remove(&id);
                *guard = None; // dropping the child kills it; the next call respawns
                return Err(EngineError::unavailable(error));
            }
            Arc::clone(&running.pending)
        };

        match tokio::time::timeout(REQUEST_TIMEOUT, receiver).await {
            Ok(Ok(reply)) => reply,
            Ok(Err(_)) => Err(EngineError::stopped()),
            Err(_) => {
                lock(&pending).remove(&id);
                self.restart_if_current(&pending).await;
                Err(EngineError::new(
                    "timeout",
                    format!(
                        "The engine did not answer within {} seconds and was restarted.",
                        REQUEST_TIMEOUT.as_secs()
                    ),
                ))
            }
        }
    }

    fn ensure_running<'a>(
        &self,
        slot: &'a mut Option<Running>,
    ) -> Result<&'a mut Running, EngineError> {
        let exited = match slot.as_mut() {
            Some(running) => !matches!(running.child.try_wait(), Ok(None)),
            None => true,
        };
        if exited {
            *slot = Some(self.spawn()?);
        }
        Ok(slot.as_mut().expect("engine slot was just filled"))
    }

    fn spawn(&self) -> Result<Running, EngineError> {
        let mut command = Command::new(&self.binary);
        command
            .arg("--content")
            .arg(&self.content)
            .args(
                self.user
                    .iter()
                    .flat_map(|user| [std::ffi::OsStr::new("--user"), user.as_os_str()]),
            )
            .stdin(Stdio::piped())
            .stdout(Stdio::piped())
            .stderr(Stdio::piped())
            .kill_on_drop(true);

        #[cfg(windows)]
        {
            // No console window flashing up behind the app.
            const CREATE_NO_WINDOW: u32 = 0x0800_0000;
            command.creation_flags(CREATE_NO_WINDOW);

            // SQLite's native library ships as a resource beside content.db.
            // In an installed app that is also the executable's directory, but
            // relying on that coincidence broke the development bridge, so
            // the directory is put on the child's DLL search path explicitly.
            if let Some(native_dir) = self.content.parent() {
                let mut path = std::ffi::OsString::from(native_dir);
                if let Some(existing) = std::env::var_os("PATH") {
                    path.push(";");
                    path.push(existing);
                }
                command.env("PATH", path);
            }
        }

        let mut child = command.spawn().map_err(|error| {
            EngineError::unavailable(format!("{error} ({})", self.binary.display()))
        })?;
        let stdin = child
            .stdin
            .take()
            .ok_or_else(|| EngineError::unavailable("no stdin"))?;
        let stdout = child
            .stdout
            .take()
            .ok_or_else(|| EngineError::unavailable("no stdout"))?;
        let stderr = child
            .stderr
            .take()
            .ok_or_else(|| EngineError::unavailable("no stderr"))?;

        let pending: Pending = Arc::default();
        tauri::async_runtime::spawn(read_responses(stdout, Arc::clone(&pending)));
        tauri::async_runtime::spawn(async move {
            let mut lines = BufReader::new(stderr).lines();
            while let Ok(Some(line)) = lines.next_line().await {
                eprintln!("[engine] {line}");
            }
        });

        Ok(Running {
            child,
            stdin,
            pending,
        })
    }

    /// Kills the engine that owns `pending`, unless it has already been replaced.
    async fn restart_if_current(&self, pending: &Pending) {
        let mut guard = self.running.lock().await;
        if guard
            .as_ref()
            .is_some_and(|running| Arc::ptr_eq(&running.pending, pending))
        {
            *guard = None;
        }
    }
}

async fn read_responses(stdout: ChildStdout, pending: Pending) {
    let mut lines = BufReader::new(stdout).lines();
    while let Ok(Some(line)) = lines.next_line().await {
        match parse_response(&line) {
            Some((id, reply)) => {
                if let Some(sender) = lock(&pending).remove(&id) {
                    // The caller may have timed out and gone; that is fine.
                    let _ = sender.send(reply);
                }
            }
            None => eprintln!("[engine] unmatched response: {line}"),
        }
    }

    // The process is gone. Dropping the senders wakes every waiter with
    // `engine_stopped`.
    lock(&pending).clear();
}

/// Splits a response line into its id and either the result or the error.
fn parse_response(line: &str) -> Option<(u64, Reply)> {
    let mut value: Value = serde_json::from_str(line).ok()?;
    let id = value.get("id")?.as_u64()?;

    if let Some(error) = value.get("error") {
        let code = error
            .get("code")
            .and_then(Value::as_str)
            .unwrap_or("internal");
        let message = error
            .get("message")
            .and_then(Value::as_str)
            .unwrap_or("The engine reported an error.");
        return Some((id, Err(EngineError::new(code, message))));
    }

    let result = value
        .get_mut("result")
        .map(Value::take)
        .unwrap_or(Value::Null);
    Some((id, Ok(result)))
}

/// Method names are short dotted identifiers; anything else never reaches the engine.
fn validate_method(method: &str) -> Result<(), EngineError> {
    let valid = !method.is_empty()
        && method.len() <= MAX_METHOD_LENGTH
        && method.bytes().all(|b| b.is_ascii_lowercase() || b == b'.');
    if valid {
        Ok(())
    } else {
        Err(EngineError::new(
            "unknown_method",
            "That is not a valid engine method name.",
        ))
    }
}

/// A poisoned lock only means another task panicked mid-insert; the map is
/// still usable, so recover it rather than propagate the panic.
fn lock<T>(mutex: &SyncMutex<T>) -> std::sync::MutexGuard<'_, T> {
    mutex
        .lock()
        .unwrap_or_else(std::sync::PoisonError::into_inner)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parses_a_result() {
        let (id, reply) = parse_response(r#"{"id":4,"result":{"verseCount":6236}}"#).unwrap();
        assert_eq!(id, 4);
        assert_eq!(reply.unwrap()["verseCount"], 6236);
    }

    #[test]
    fn parses_an_error() {
        let (id, reply) =
            parse_response(r#"{"id":5,"error":{"code":"not_found","message":"No such system."}}"#)
                .unwrap();
        assert_eq!(id, 5);
        assert_eq!(
            reply.unwrap_err(),
            EngineError::new("not_found", "No such system.")
        );
    }

    #[test]
    fn a_null_id_or_garbage_is_unmatched() {
        assert!(
            parse_response(r#"{"id":null,"error":{"code":"parse_error","message":"x"}}"#).is_none()
        );
        assert!(parse_response("not json").is_none());
    }

    #[test]
    fn method_names_are_restricted() {
        assert!(validate_method("selection.stats").is_ok());
        assert!(validate_method("").is_err());
        assert!(validate_method("Selection.Stats").is_err());
        assert!(validate_method("x\n{\"id\":1}").is_err());
        assert!(validate_method(&"a".repeat(65)).is_err());
    }

    /// The sidecar staged by `pnpm engine`, if it has been built.
    fn staged_engine() -> Option<Engine> {
        let root = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
        let triple = if cfg!(all(windows, target_arch = "x86_64")) {
            "x86_64-pc-windows-msvc"
        } else {
            return None;
        };
        let binary = root.join(format!(
            "binaries/qurancode-engine-{triple}{}",
            std::env::consts::EXE_SUFFIX
        ));
        let content = root.join("resources/content.db");
        (binary.exists() && content.exists()).then(|| Engine::new(binary, content, None))
    }

    #[tokio::test]
    async fn round_trips_through_the_real_sidecar() {
        let Some(engine) = staged_engine() else {
            return;
        };

        // Holds for whichever edition is staged: classic 6236 rows, Submission 6346.
        let info = engine.call("engine.info", None).await.unwrap();
        assert_eq!(info["chapterCount"], 114);
        assert!(info["rowCount"].as_u64().unwrap() >= info["verseCount"].as_u64().unwrap());

        let error = engine
            .call("chapter.verses", Some(json!({ "chapter": 115 })))
            .await
            .unwrap_err();
        assert_eq!(error.code, "invalid_params");
    }

    #[tokio::test]
    async fn respawns_after_the_engine_dies() {
        let Some(engine) = staged_engine() else {
            return;
        };
        engine.call("engine.info", None).await.unwrap();

        {
            let mut guard = engine.running.lock().await;
            let running = guard.as_mut().unwrap();
            running.child.start_kill().unwrap();
            running.child.wait().await.unwrap();
        }

        let info = engine.call("engine.info", None).await.unwrap();
        assert_eq!(info["chapterCount"], 114);
    }

    #[tokio::test]
    async fn a_missing_binary_is_reported_not_panicked() {
        let engine = Engine::new(
            PathBuf::from("definitely-not-here.exe"),
            PathBuf::from("content.db"),
            None,
        );
        let error = engine.call("engine.info", None).await.unwrap_err();
        assert_eq!(error.code, "engine_unavailable");
    }
}
