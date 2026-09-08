import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";
import postcssRTLCSS from "postcss-rtlcss";
import { Mode } from "postcss-rtlcss/options";
import path from "path";

/**
 * Build config for the **standalone** media gallery — a self-contained SPA (its own index.html +
 * OIDC callback pages) hosted on its own origin against a remote Orchard tenant. This is separate
 * from vite.config.ts, which builds the library bundle Orchard consumes via `asp-name="media"`.
 *
 * Output: dist-standalone/ (index.html, callback.html, silent-callback.html + hashed assets).
 * A deployment supplies a config.json (see standalone/config.example.json) next to index.html.
 */
export default defineConfig({
  root: path.resolve(__dirname, "standalone"),
  // Relative asset URLs so dist-standalone/ works from any sub-path, not just an origin root
  // (safe: the app router is hash-based, so the document URL never varies within a deployment).
  base: "./",
  resolve: {
    alias: {
      vue: "vue/dist/vue.esm-bundler.js",
      "@bloom": path.resolve(__dirname, "../../../../../.scripts/bloom"),
    },
  },
  plugins: [tailwindcss(), vue()],
  css: {
    postcss: {
      plugins: [postcssRTLCSS({ mode: Mode.combined }) as never],
    },
  },
  define: {
    "process.env.NODE_ENV": JSON.stringify("production"),
  },
  build: {
    outDir: path.resolve(__dirname, "dist-standalone"),
    emptyOutDir: true,
    rollupOptions: {
      input: {
        index: path.resolve(__dirname, "standalone/index.html"),
        callback: path.resolve(__dirname, "standalone/callback.html"),
        "silent-callback": path.resolve(__dirname, "standalone/silent-callback.html"),
      },
    },
  },
});
