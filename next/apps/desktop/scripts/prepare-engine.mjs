// Builds the engine sidecar with Native AOT and stages everything Tauri
// bundles: the sidecar binary (named with the Rust target triple, as Tauri
// requires), SQLite's native library, and content.db.
//
// Skips the .NET build when the staged binary is newer than every engine
// source file, so `pnpm app` stays fast after the first run.

import { execFileSync } from "node:child_process";
import { copyFileSync, existsSync, mkdirSync, readdirSync, statSync } from "node:fs";
import { homedir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const desktop = resolve(here, "..");
const next = resolve(desktop, "../..");
const engineProject = join(next, "apps/QuranCode.Engine");
// The app ships the Submission edition. QURANCODE_EDITION=classic stages the
// legacy Tanzil text instead, for comparing against the original software.
const EDITIONS = { submission: "data/submission.db", classic: "data/content.db" };
const edition = process.env.QURANCODE_EDITION ?? "submission";
const contentDb = EDITIONS[edition] ? join(next, EDITIONS[edition]) : null;
const tauriDir = join(desktop, "src-tauri");
const binariesDir = join(tauriDir, "binaries");
const resourcesDir = join(tauriDir, "resources");
const publishDir = join(tauriDir, "target/engine-publish");

const RIDS = {
  "x86_64-pc-windows-msvc": "win-x64",
  "aarch64-pc-windows-msvc": "win-arm64",
  "x86_64-unknown-linux-gnu": "linux-x64",
  "aarch64-unknown-linux-gnu": "linux-arm64",
  "x86_64-apple-darwin": "osx-x64",
  "aarch64-apple-darwin": "osx-arm64",
};

const SQLITE_LIBRARY = { win: "e_sqlite3.dll", linux: "libe_sqlite3.so", osx: "libe_sqlite3.dylib" };

function fail(message) {
  console.error(`prepare-engine: ${message}`);
  process.exit(1);
}

function hostTriple() {
  const info = execFileSync("rustc", ["-vV"], { encoding: "utf8" });
  const host = info.split("\n").find((line) => line.startsWith("host:"));
  if (!host) fail("could not read the host target from `rustc -vV`.");
  return host.slice("host:".length).trim();
}

// Resolving the executable rather than trusting a bare "dotnet" to be found on
// PATH. GitHub's windows runners failed here with `spawnSync dotnet ENOENT`
// even though actions/setup-dotnet had run and DOTNET_ROOT was set, while the
// same workflow's `dotnet test` steps (which go through a shell) worked. The
// exact PATH condition on the runner was never reproduced locally, so this
// checks every place the executable is known to live instead of guessing which
// one failed.
function findDotnet() {
  const exe = process.platform === "win32" ? "dotnet.exe" : "dotnet";
  const candidates = [
    process.env.DOTNET,
    process.env.DOTNET_ROOT ? join(process.env.DOTNET_ROOT, exe) : null,
    // setup-dotnet installs here when it cannot write to the shared location.
    process.env.DOTNET_INSTALL_DIR ? join(process.env.DOTNET_INSTALL_DIR, exe) : null,
    join(homedir(), ".dotnet", exe),
  ];
  for (const candidate of candidates) {
    if (candidate && existsSync(candidate)) return candidate;
  }
  // Nothing absolute found: fall back to PATH lookup, which works locally.
  return "dotnet";
}

function newestSourceTime(roots) {
  let newest = 0;
  const visit = (dir) => {
    for (const entry of readdirSync(dir, { withFileTypes: true })) {
      if (entry.name === "bin" || entry.name === "obj") continue;
      const path = join(dir, entry.name);
      if (entry.isDirectory()) visit(path);
      else if (/\.(cs|csproj)$/.test(entry.name)) newest = Math.max(newest, statSync(path).mtimeMs);
    }
  };
  roots.forEach(visit);
  return newest;
}

function publish(rid) {
  const env = { ...process.env, DOTNET_CLI_TELEMETRY_OPTOUT: "1", DOTNET_NOLOGO: "1" };
  if (process.platform === "win32") {
    // The AOT linker locates MSVC through vswhere, which the VS installer
    // does not put on PATH.
    const installer = join(process.env["ProgramFiles(x86)"] ?? "C:\\Program Files (x86)", "Microsoft Visual Studio", "Installer");
    // Extend the existing key whatever its case. Windows treats environment
    // names case-insensitively, but a spread of process.env is a plain object
    // that keeps the OS spelling, which is "Path" as often as "PATH". Assigning
    // to env.PATH when the copy holds "Path" adds a second key instead of
    // extending the first, and the child then inherits two, one of which holds
    // only the directory added here.
    const pathKey = Object.keys(env).find((k) => k.toLowerCase() === "path") ?? "PATH";
    env[pathKey] = `${installer};${env[pathKey] ?? ""}`;
  }
  console.log(`prepare-engine: publishing ${rid} with Native AOT`);
  execFileSync(findDotnet(), ["publish", engineProject, "-c", "Release", "-r", rid, "-o", publishDir, "--nologo"], {
    stdio: "inherit",
    env,
  });
}

const triple = hostTriple();
const rid = RIDS[triple] ?? fail(`no .NET runtime identifier is mapped for ${triple}.`);
const platform = rid.split("-")[0];
const exe = platform === "win" ? ".exe" : "";

if (!contentDb) fail(`unknown QURANCODE_EDITION "${edition}"; use ${Object.keys(EDITIONS).join(" or ")}.`);
if (!existsSync(contentDb)) {
  const how =
    edition === "submission"
      ? "python next/data/import/build_content.py --edition submission -o next/data/submission.db"
      : "python next/data/import/build_content.py -o next/data/content.db";
  fail(`${contentDb} is missing. Build it with: ${how}`);
}

const staged = join(binariesDir, `qurancode-engine-${triple}${exe}`);
const sources = newestSourceTime([engineProject, join(next, "src/QuranCode.Core")]);
const fresh = existsSync(staged) && statSync(staged).mtimeMs > sources;

mkdirSync(binariesDir, { recursive: true });
mkdirSync(resourcesDir, { recursive: true });

if (fresh) {
  console.log("prepare-engine: sidecar is up to date");
} else {
  publish(rid);
  try {
    copyFileSync(join(publishDir, `qurancode-engine${exe}`), staged);
    copyFileSync(join(publishDir, SQLITE_LIBRARY[platform]), join(resourcesDir, SQLITE_LIBRARY[platform]));
  } catch (error) {
    if (error?.code === "EBUSY" || error?.code === "EPERM") {
      fail("the staged engine is in use. Stop `pnpm dev` or the running app, then run this again.");
    }
    throw error;
  }
}

copyFileSync(contentDb, join(resourcesDir, "content.db"));
console.log(`prepare-engine: staged ${staged} with the ${edition} edition`);
