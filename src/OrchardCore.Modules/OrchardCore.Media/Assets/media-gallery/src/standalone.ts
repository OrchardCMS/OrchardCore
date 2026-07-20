declare global {
  var __VUE_PROD_DEVTOOLS__: boolean; // eslint-disable-line no-var
}

globalThis.__VUE_PROD_DEVTOOLS__ = false;

import { createApp, h } from "vue";
import AppComponent from "./App.vue";
import { registerNotificationBus } from "@bloom/services/notifications/notifier";
import { configureMediaApp } from "./appSetup";
// Standalone-only base layer (box model, typography, page colors) — replaces what Bootstrap/the
// admin theme supplied when embedded. Not imported by the embedded entry, so the admin is unaffected.
import "./assets/css/standalone.css";
import {
  resolveStandaloneConfig,
  setRuntimeConfig,
  type IStandaloneConfigSource,
  type IMediaRuntimeConfig,
} from "./services/RuntimeConfig";

/**
 * Entry point for the **standalone** media gallery — the app hosted on its own origin against a
 * remote OrchardCore tenant. Unlike the embedded entry (main.ts), there is no Razor host to inject
 * `<media-gallery>` attributes: config comes from a fetched `config.json`, and the App is rendered
 * with an already-resolved two-origin runtime config. Auth is bearer + interactive (handled inside
 * App.vue from the injected config).
 */

const CONFIG_URL = "config.json";

async function loadConfigSource(): Promise<IStandaloneConfigSource> {
  const response = await fetch(CONFIG_URL, { cache: "no-store" });
  if (!response.ok) {
    throw new Error(`Failed to load ${CONFIG_URL}: ${response.status} ${response.statusText}`);
  }
  const source = (await response.json()) as IStandaloneConfigSource;
  if (!source.orchardBaseUrl) {
    throw new Error(`${CONFIG_URL} is missing the required "orchardBaseUrl" field.`);
  }
  return source;
}

/** Load the media gallery's UI labels from the remote Orchard tenant (the same "media-gallery" JS
 * localizations the embedded admin page renders, for the server's resolved culture). The endpoint is
 * anonymous, so this runs before authentication. Falls back to an empty set on failure. */
async function loadTranslations(apiBaseUrl: string): Promise<string> {
  try {
    const base = apiBaseUrl.endsWith("/") ? apiBaseUrl : `${apiBaseUrl}/`;
    const response = await fetch(`${base}api/media/localizations`, { cache: "no-store" });
    if (response.ok) {
      return JSON.stringify(await response.json());
    }
  } catch {
    // Endpoint unreachable — fall back to empty (labels use their built-in fallbacks where present).
  }
  return "{}";
}

function mount(config: IMediaRuntimeConfig, translations: string, signalrEnabled: boolean): void {
  const container = document.getElementById("media-gallery");
  if (!container) {
    throw new Error('Standalone host is missing a <div id="media-gallery"> mount point.');
  }

  const app = createApp({
    name: "media-standalone",
    render: () =>
      h(AppComponent, {
        // Inject the resolved two-origin config; App.vue uses it instead of resolving from attributes.
        runtimeConfig: config,
        // basePath is required by App.vue but unused for config when runtimeConfig is provided.
        basePath: config.orchardBaseUrl,
        translations,
        uploadFilesUrl: `${config.apiBaseUrl}api/media/Upload`,
        maxUploadChunkSize: 0,
        allowMultipleSelection: true,
        // Real-time updates need the SignalR CORS surface on the Orchard origin; opt in via config.
        signalrEnabled: signalrEnabled ? "true" : "false",
      }),
  });

  configureMediaApp(app);
  registerNotificationBus();
  app.mount(container);
}

async function bootstrap(): Promise<void> {
  try {
    const source = await loadConfigSource();
    const config = resolveStandaloneConfig(source);
    setRuntimeConfig(config);
    const translations = await loadTranslations(config.apiBaseUrl);
    mount(config, translations, Boolean(source.signalrEnabled));
  } catch (err) {
    console.error("[media-gallery] standalone bootstrap failed", err);
    const container = document.getElementById("media-gallery");
    if (container) {
      container.textContent = `Media gallery failed to start: ${(err as Error).message}`;
    }
  }
}

void bootstrap();
