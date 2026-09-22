/// <reference types="vitest/config" />
import { svelte } from "@sveltejs/vite-plugin-svelte";
import { fileURLToPath } from "node:url";
import { defineConfig } from "vite";
import { engineDevBridge } from "./scripts/engine-dev-bridge.ts";

const tauriDir = fileURLToPath(new URL("./src-tauri", import.meta.url));

export default defineConfig({
  plugins: [svelte(), engineDevBridge(tauriDir)],
  clearScreen: false,
  server: {
    port: 1420,
    strictPort: true,
    watch: { ignored: ["**/src-tauri/**"] },
  },
  build: {
    target: "es2022",
    sourcemap: false,
  },
  test: {
    include: ["src/**/*.test.ts"],
  },
});
