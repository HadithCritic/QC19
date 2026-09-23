mod engine;

use std::path::PathBuf;

use engine::{Engine, EngineError};
use serde_json::Value;
use tauri::Manager;

/// The main command the webview may call. Everything the UI does goes through
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
            // Bookmarks and history live in the per-user app data folder,
            // apart from the read-only content that ships with the app.
            let data = app.path().app_data_dir()?;
            let user = data.join("user.db");
            app.manage(Engine::new(sidecar_path()?, content, Some(user)));

            // Start the engine now, so it warms up while the window loads
            // instead of when the first screen asks for data.
            let handle = app.handle().clone();
            tauri::async_runtime::spawn(async move {
                if let Err(error) = handle.state::<Engine>().call("engine.info", None).await {
                    eprintln!("[engine] could not start early: {}", error.message);
                }
            });
            Ok(())
        })
        .invoke_handler(tauri::generate_handler![engine])
        .run(tauri::generate_context!())
        .expect("error while running QuranCode");
}
