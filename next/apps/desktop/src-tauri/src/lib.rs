mod engine;

use std::path::PathBuf;

use engine::{Engine, EngineError};
use serde_json::Value;
use tauri::Manager;

/// The one command the webview may call. Everything the UI does goes through
/// the engine's own method table, which validates its parameters.
#[tauri::command]
async fn engine(
    state: tauri::State<'_, Engine>,
    method: String,
    params: Option<Value>,
) -> Result<Value, EngineError> {
    state.call(&method, params).await
}

/// The sidecar sits next to the app executable, both in development (Tauri
/// copies it into the target directory) and in an installed bundle.
fn sidecar_path() -> std::io::Result<PathBuf> {
    let exe = std::env::current_exe()?;
    let dir = exe
        .parent()
        .ok_or_else(|| std::io::Error::other("the app executable has no parent directory"))?;
    Ok(dir.join(format!("qurancode-engine{}", std::env::consts::EXE_SUFFIX)))
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .setup(|app| {
            let content = app.path().resource_dir()?.join("content.db");
            app.manage(Engine::new(sidecar_path()?, content));
            Ok(())
        })
        .invoke_handler(tauri::generate_handler![engine])
        .run(tauri::generate_context!())
        .expect("error while running QuranCode");
}
