// Development only: lets the UI run in a plain browser against the real
// engine, over the same line protocol the Rust bridge speaks. Vite applies it
// to `vite dev` and never to a build.

import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { existsSync } from "node:fs";
import type { IncomingMessage, ServerResponse } from "node:http";
import { delimiter, dirname, join } from "node:path";
import { createInterface } from "node:readline";
import type { Plugin } from "vite";

const MAX_BODY = 1 << 20;

interface Reply {
  status: number;
  body: unknown;
}

class DevEngine {
  private child: ChildProcessWithoutNullStreams | null = null;
  private nextId = 1;
  private readonly pending = new Map<number, (reply: Reply) => void>();

  constructor(
    private readonly binary: string,
    private readonly content: string,
  ) {}

  call(method: string, params: unknown): Promise<Reply> {
    const child = this.ensure();
    const id = this.nextId++;
    return new Promise((resolve) => {
      this.pending.set(id, resolve);
      child.stdin.write(`${JSON.stringify({ id, method, params })}\n`);
    });
  }

  private ensure(): ChildProcessWithoutNullStreams {
    if (this.child && this.child.exitCode === null) return this.child;

    // SQLite's native library is staged beside content.db, not beside the
    // binary; put that directory on the DLL search path, as the Rust bridge does.
    const nativeDir = dirname(this.content);
    const env = { ...process.env, PATH: `${nativeDir}${delimiter}${process.env.PATH ?? ""}` };
    // Development user data sits in the ignored target folder, never in the source tree.
    const user = join(dirname(dirname(this.content)), "target", "dev-user.db");
    // The translation pack built in next/data, when there is one (build_translations.py).
    const pack = process.env.QURANCODE_TRANSLATIONS ?? join(dirname(dirname(dirname(dirname(this.content)))), "..", "data", "submission-translations.db");
    const packs = existsSync(pack) ? ["--translations", pack] : [];
    const child = spawn(this.binary, ["--content", this.content, "--user", user, ...packs], { stdio: "pipe", env });
    createInterface({ input: child.stdout }).on("line", (line) => this.receive(line));
    createInterface({ input: child.stderr }).on("line", (line) => console.error(`[engine] ${line}`));
    child.on("exit", () => {
      for (const resolve of this.pending.values()) {
        resolve({ status: 502, body: { code: "engine_stopped", message: "The engine stopped unexpectedly." } });
      }
      this.pending.clear();
      this.child = null;
    });
    this.child = child;
    return child;
  }

  private receive(line: string): void {
    const response = JSON.parse(line) as { id: number | null; result?: unknown; error?: unknown };
    if (response.id === null) return;
    const resolve = this.pending.get(response.id);
    this.pending.delete(response.id);
    resolve?.(response.error ? { status: 422, body: response.error } : { status: 200, body: response.result ?? null });
  }
}

function readBody(request: IncomingMessage): Promise<string> {
  return new Promise((resolve, reject) => {
    let body = "";
    request.on("data", (chunk: Buffer) => {
      body += chunk.toString("utf8");
      if (body.length > MAX_BODY) reject(new Error("request too large"));
    });
    request.on("end", () => resolve(body));
    request.on("error", reject);
  });
}

function send(response: ServerResponse, status: number, body: unknown): void {
  response.statusCode = status;
  response.setHeader("Content-Type", "application/json; charset=utf-8");
  response.end(JSON.stringify(body));
}

export function engineDevBridge(tauriDir: string, triple = "x86_64-pc-windows-msvc"): Plugin {
  const exe = process.platform === "win32" ? ".exe" : "";
  const binary = join(tauriDir, "binaries", `qurancode-engine-${triple}${exe}`);
  const content = join(tauriDir, "resources", "content.db");

  return {
    name: "qurancode-engine-dev-bridge",
    apply: "serve",
    configureServer(server) {
      if (!existsSync(binary) || !existsSync(content)) {
        server.config.logger.warn("engine not staged; run `pnpm engine` to use the UI in a browser");
        return;
      }
      const engine = new DevEngine(binary, content);

      server.middlewares.use("/__engine", (request, response) => {
        // JSON content type forces a CORS preflight, which this server never
        // answers, so other sites cannot reach the engine from a browser tab.
        const origin = request.headers.origin;
        const host = request.headers.host;
        const sameOrigin = !origin || origin === `http://${host}`;
        const isJson = request.headers["content-type"]?.startsWith("application/json") ?? false;
        if (request.method !== "POST" || !sameOrigin || !isJson) {
          send(response, 403, { code: "forbidden", message: "Not allowed." });
          return;
        }

        readBody(request)
          .then(async (raw) => {
            const { method, params } = JSON.parse(raw) as { method: string; params?: unknown };
            const reply = await engine.call(method, params ?? null);
            send(response, reply.status, reply.body);
          })
          .catch((error: unknown) => send(response, 400, { code: "parse_error", message: String(error) }));
      });
    },
  };
}
