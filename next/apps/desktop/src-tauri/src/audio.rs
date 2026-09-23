//! Recitation audio (Features.txt #65): verse files from everyayah.com,
//! downloaded on first use and kept in the app's data folder, as the original
//! keeps them under Audio/<reciter>/<CCC>/<CCCVVV>.mp3.

use std::path::{Path, PathBuf};
use std::time::Duration;

/// Where the recordings come from; the original's default prefix.
const SOURCE: &str = "https://everyayah.com/data";

/// A real verse recording is larger than this; the original treats smaller
/// files as failed downloads and fetches them again.
const MIN_BYTES: usize = 1024;

/// The original gives a download 30 seconds.
const TIMEOUT: Duration = Duration::from_secs(30);

#[derive(Debug, PartialEq, Eq)]
pub struct AudioError {
    pub code: &'static str,
    pub message: String,
}

impl AudioError {
    fn new(code: &'static str, message: impl Into<String>) -> Self {
        Self {
            code,
            message: message.into(),
        }
    }
}

/// A reciter folder as the catalog writes them: letters, digits, `_` and `-`,
/// at most one `/` (MultiLanguage/Basfar_Walk_192kbps). Nothing that could
/// leave the audio folder.
pub fn valid_folder(folder: &str) -> bool {
    let parts: Vec<&str> = folder.split('/').collect();
    !folder.is_empty()
        && folder.len() <= 80
        && parts.len() <= 2
        && parts.iter().all(|p| {
            !p.is_empty()
                && p.chars()
                    .all(|c| c.is_ascii_alphanumeric() || c == '_' || c == '-')
        })
}

/// A verse file name: chapter and verse as six digits, 001001 to 114006.
pub fn valid_name(name: &str) -> bool {
    name.len() == 6 && name.chars().all(|c| c.is_ascii_digit()) && &name[..3] != "000"
}

/// The cached file for a verse: <root>/<folder>/<CCC>/<CCCVVV>.mp3.
pub fn cache_path(root: &Path, folder: &str, name: &str) -> PathBuf {
    let mut path = root.to_path_buf();
    for part in folder.split('/') {
        path.push(part);
    }
    path.push(&name[..3]);
    path.push(format!("{name}.mp3"));
    path
}

/// A verse recording: from the cache, or downloaded and cached.
pub async fn verse_audio(root: &Path, folder: &str, name: &str) -> Result<Vec<u8>, AudioError> {
    if !valid_folder(folder) {
        return Err(AudioError::new(
            "invalid_params",
            "That is not a reciter folder.",
        ));
    }
    if !valid_name(name) {
        return Err(AudioError::new(
            "invalid_params",
            "A verse file is named by six digits, as 002255.",
        ));
    }

    let path = cache_path(root, folder, name);
    if let Ok(bytes) = tokio::fs::read(&path).await {
        if bytes.len() >= MIN_BYTES {
            return Ok(bytes);
        }
    }

    let url = format!("{SOURCE}/{folder}/{name}.mp3");
    let bytes = download(&url).await?;

    // Write beside the target, then move it into place, so an interrupted
    // write never leaves a short file that looks like a recording.
    if let Some(parent) = path.parent() {
        tokio::fs::create_dir_all(parent).await.map_err(|e| {
            AudioError::new(
                "cache_failed",
                format!("The audio folder could not be made: {e}"),
            )
        })?;
    }
    let partial = path.with_extension("part");
    tokio::fs::write(&partial, &bytes).await.map_err(|e| {
        AudioError::new(
            "cache_failed",
            format!("The recording could not be saved: {e}"),
        )
    })?;
    tokio::fs::rename(&partial, &path).await.map_err(|e| {
        AudioError::new(
            "cache_failed",
            format!("The recording could not be saved: {e}"),
        )
    })?;
    Ok(bytes)
}

async fn download(url: &str) -> Result<Vec<u8>, AudioError> {
    let client = reqwest::Client::builder()
        .timeout(TIMEOUT)
        .build()
        .map_err(|e| AudioError::new("download_failed", e.to_string()))?;
    let response = client.get(url).send().await.map_err(|e| {
        AudioError::new(
            "download_failed",
            format!("The recording could not be fetched: {e}"),
        )
    })?;
    if !response.status().is_success() {
        return Err(AudioError::new(
            "download_failed",
            format!(
                "everyayah.com answered {} for this recording.",
                response.status()
            ),
        ));
    }
    let audio = response
        .headers()
        .get(reqwest::header::CONTENT_TYPE)
        .and_then(|v| v.to_str().ok())
        .map_or(true, |t| {
            t.starts_with("audio/") || t.starts_with("application/octet-stream")
        });
    let bytes = response.bytes().await.map_err(|e| {
        AudioError::new("download_failed", format!("The recording was cut off: {e}"))
    })?;
    if !audio || bytes.len() < MIN_BYTES {
        return Err(AudioError::new(
            "download_failed",
            "What came back is not a recording.",
        ));
    }
    Ok(bytes.to_vec())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn folders_stay_inside_the_audio_folder() {
        assert!(valid_folder("Alafasy_64kbps"));
        assert!(valid_folder("MultiLanguage/Basfar_Walk_192kbps"));
        assert!(!valid_folder("../secrets"));
        assert!(!valid_folder("a/b/c"));
        assert!(!valid_folder("/etc"));
        assert!(!valid_folder("C:\\Windows"));
        assert!(!valid_folder(""));
    }

    #[test]
    fn names_are_six_digits() {
        assert!(valid_name("002255"));
        assert!(!valid_name("2255"));
        assert!(!valid_name("00225a"));
        assert!(!valid_name("000001"));
    }

    #[test]
    fn the_cache_nests_by_chapter() {
        let path = cache_path(
            Path::new("root"),
            "MultiLanguage/Basfar_Walk_192kbps",
            "002255",
        );
        assert!(path.ends_with(
            Path::new("MultiLanguage")
                .join("Basfar_Walk_192kbps")
                .join("002")
                .join("002255.mp3")
        ));
    }
}
