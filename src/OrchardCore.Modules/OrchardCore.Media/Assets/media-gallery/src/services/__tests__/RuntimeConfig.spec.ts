import { describe, it, expect, beforeEach } from "vitest";
import {
  resolveEmbeddedConfig,
  resolveStandaloneConfig,
  setRuntimeConfig,
  useRuntimeConfig,
} from "../RuntimeConfig";

// window.location.origin is "http://localhost:3000" under jsdom.
const ORIGIN = window.location.origin;

describe("resolveEmbeddedConfig", () => {
  it("collapses both origins to the current origin and preserves the pre-refactor API base + hub URL", () => {
    const config = resolveEmbeddedConfig({ basePath: "/" });

    expect(config.appBaseUrl).toBe(`${ORIGIN}/`);
    expect(config.orchardBaseUrl).toBe(`${ORIGIN}/`);
    // API base stays the exact relative value the MediaApiClient has always received.
    expect(config.apiBaseUrl).toBe("/");
    // Hub keeps its exact same-origin path (no tenant-prefix behavior change).
    expect(config.hubUrl).toBe("/hubs/media");
    expect(config.authFlow).toBe("silent");
  });

  it("defaults auth scheme to Cookie and only switches to Bearer when explicitly set", () => {
    expect(resolveEmbeddedConfig({ basePath: "/" }).apiAuthScheme).toBe("Cookie");
    expect(resolveEmbeddedConfig({ basePath: "/", apiAuthScheme: "Bearer" }).apiAuthScheme).toBe("Bearer");
    expect(resolveEmbeddedConfig({ basePath: "/", apiAuthScheme: "anything" }).apiAuthScheme).toBe("Cookie");
  });

  it("derives authority and the silent-renew redirect URI from the app origin + base", () => {
    const config = resolveEmbeddedConfig({ basePath: "/team-a/" });

    expect(config.authority).toBe(`${ORIGIN}/team-a`);
    expect(config.redirectUri).toBe(`${ORIGIN}/team-a/OrchardCore.Media/media-gallery-oidc-silent.html`);
    expect(config.silentRedirectUri).toBe(config.redirectUri);
  });

  it("normalizes a basePath without a trailing slash", () => {
    const config = resolveEmbeddedConfig({ basePath: "/team-a" });
    expect(config.appBaseUrl).toBe(`${ORIGIN}/team-a/`);
  });

  it("honors explicit OIDC overrides", () => {
    const config = resolveEmbeddedConfig({
      basePath: "/",
      oidcAuthority: "https://id.example.com",
      oidcClientId: "custom_client",
      oidcScope: "openid roles",
    });

    expect(config.authority).toBe("https://id.example.com");
    expect(config.clientId).toBe("custom_client");
    expect(config.scope).toBe("openid roles");
  });
});

describe("resolveStandaloneConfig", () => {
  it("separates the app origin (redirect URIs) from the Orchard origin (issuer/API/hub)", () => {
    const config = resolveStandaloneConfig({
      orchardBaseUrl: "https://cms.example.com/",
      appBaseUrl: "https://media.example.com/",
    });

    expect(config.appBaseUrl).toBe("https://media.example.com/");
    expect(config.orchardBaseUrl).toBe("https://cms.example.com/");
    expect(config.apiBaseUrl).toBe("https://cms.example.com/");
    expect(config.hubUrl).toBe("https://cms.example.com/hubs/media");
    expect(config.authority).toBe("https://cms.example.com");
    // Callback pages live on the app origin.
    expect(config.redirectUri).toBe("https://media.example.com/callback.html");
    expect(config.silentRedirectUri).toBe("https://media.example.com/silent-callback.html");
    // Standalone is always bearer + interactive.
    expect(config.apiAuthScheme).toBe("Bearer");
    expect(config.authFlow).toBe("interactive");
  });

  it("preserves a tenant prefix on the Orchard origin", () => {
    const config = resolveStandaloneConfig({ orchardBaseUrl: "https://cms.example.com/team-a/" });
    expect(config.apiBaseUrl).toBe("https://cms.example.com/team-a/");
    expect(config.hubUrl).toBe("https://cms.example.com/team-a/hubs/media");
    expect(config.authority).toBe("https://cms.example.com/team-a");
  });

  it("strips the document filename when defaulting the app base from the page URL", () => {
    window.history.replaceState(null, "", "/subdir/index.html");
    try {
      const config = resolveStandaloneConfig({ orchardBaseUrl: "https://cms.example.com/" });
      // Without the strip, the redirect URIs would be ".../index.html/callback.html" — matching no
      // registered RedirectUri and no real page.
      expect(config.appBaseUrl).toBe(`${ORIGIN}/subdir/`);
      expect(config.redirectUri).toBe(`${ORIGIN}/subdir/callback.html`);
    } finally {
      window.history.replaceState(null, "", "/");
    }
  });
});

describe("useRuntimeConfig", () => {
  beforeEach(() => {
    setRuntimeConfig(resolveEmbeddedConfig({ basePath: "/" }));
  });

  it("returns the stored config", () => {
    const stored = resolveStandaloneConfig({ orchardBaseUrl: "https://cms.example.com/" });
    setRuntimeConfig(stored);
    expect(useRuntimeConfig()).toBe(stored);
  });
});
