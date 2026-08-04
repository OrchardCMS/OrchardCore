import {
  runRedirectCallback,
  runSilentRenewCallback,
  SilentAuthClient,
  type SilentAuthSettings,
} from "@bloom/services/auth/silent-oidc";
import { resolveEmbeddedConfig } from "./RuntimeConfig";

/**
 * OAuth2 authorization-code + PKCE authentication for the media gallery SPA.
 *
 * The silent core (token acquisition, renewal, the silent-renew callback) lives in
 * @bloom/services/auth/silent-oidc and is shared with the OpenAPI documentation UIs. This module
 * adds the media-gallery-specific layer: mapping the element/config-json config to the core's
 * settings, and the interactive (standalone) full-page login flow the silent-only OpenAPI UIs
 * don't have.
 *
 * The silent-renew iframe loads media-gallery-oidc-silent.html, which re-enters this module (see
 * main.ts) and calls handleSilentRenewCallback(). Because that page has no <media-gallery> element
 * to read config from, configureAuth() persists the resolved settings to sessionStorage so the
 * callback can rebuild a matching UserManager.
 */

const CONFIG_STORAGE_KEY = "media-gallery:oidc-settings";
const LOG_LABEL = "[media-gallery]";

/** How the bearer token is first acquired. `silent` (embedded) uses a hidden iframe against an
 * existing Orchard admin session; `interactive` (standalone) redirects to the login page. */
export type AuthFlow = "silent" | "interactive";

export interface IOidcConfig {
  /** Absolute issuer URL (tenant root); discovery is at {authority}/.well-known/openid-configuration. */
  authority: string;
  clientId: string;
  scope: string;
  /** Redirect URI for the interactive flow (equals the silent URI in embedded mode; a dedicated
   * callback page on the app origin in standalone mode). */
  redirectUri: string;
  /** Silent-renew redirect URI — must match a RedirectUris entry registered for the client. */
  silentRedirectUri: string;
  /** Token-acquisition flow; defaults to `silent` (embedded). */
  authFlow?: AuthFlow;
}

let client: SilentAuthClient | null = null;
let authFlow: AuthFlow = "silent";

/** Map the media-gallery config to the shared core's settings. The gallery persists tokens in
 * sessionStorage (the default store), so no tokenStore override is needed. */
function toSettings(config: IOidcConfig): SilentAuthSettings {
  return {
    authority: config.authority,
    clientId: config.clientId,
    scope: config.scope,
    redirectUri: config.redirectUri,
    silentRedirectUri: config.silentRedirectUri,
  };
}

/**
 * Configure silent OIDC auth. Returns false (and leaves the app unconfigured) when no
 * authority/clientId is supplied, in which case Media API calls will get a 401 until the
 * MediaApiPkce recipe is applied and config attributes are present.
 */
export function configureAuth(config: IOidcConfig): boolean {
  if (!config.authority || !config.clientId) {
    return false;
  }

  window.sessionStorage.setItem(CONFIG_STORAGE_KEY, JSON.stringify(config));

  authFlow = config.authFlow ?? "silent";
  client = new SilentAuthClient(toSettings(config), {
    label: LOG_LABEL,
    // A definitive silent failure in interactive (standalone) mode means the IdP session is gone
    // (e.g. it expired while the tab was open), so start a fresh interactive login — otherwise
    // every subsequent call would 401 until a manual reload.
    onSilentFailure: () => {
      if (authFlow === "interactive") {
        void signinRedirect();
      }
    },
  });

  return true;
}

export function isAuthConfigured(): boolean {
  return client !== null;
}

/**
 * Configure silent OIDC auth using the media gallery's conventional client id and silent-renew
 * page, derived from the current origin and base path. Shared by the gallery App and the
 * media-picker so both authenticate the same way. No-op-returns false if clientId resolves empty.
 */
export function configureMediaAuth(options: {
  basePath: string;
  authority?: string;
  clientId?: string;
  scope?: string;
}): boolean {
  // Delegate the derivation to the runtime-config resolver so the gallery and the picker share a
  // single source of truth for the OIDC defaults (client id, scope, silent page, authority).
  const config = resolveEmbeddedConfig({
    basePath: options.basePath,
    oidcAuthority: options.authority,
    oidcClientId: options.clientId,
    oidcScope: options.scope,
  });

  return configureAuth({
    authority: config.authority,
    clientId: config.clientId,
    scope: config.scope,
    redirectUri: config.redirectUri,
    silentRedirectUri: config.silentRedirectUri,
    authFlow: config.authFlow,
  });
}

/**
 * Return a valid access token, silently acquiring/renewing one if needed.
 * Used by the axios request interceptor.
 *
 * In interactive (standalone) mode, a definitive silent failure means the IdP session is gone
 * (e.g. it expired while the tab was open), so a fresh interactive login is started (see the
 * onSilentFailure hook in configureAuth) — otherwise every subsequent call would 401 until a
 * manual reload.
 */
export async function getAccessToken(): Promise<string | null> {
  return client ? client.getToken() : null;
}

/**
 * Synchronous accessor for the last-acquired token. Used where an async provider is not
 * accepted (e.g. the SignalR access-token factory). May be empty before the first
 * getAccessToken() resolves.
 */
export function getAccessTokenSync(): string {
  return client?.getTokenSync() ?? "";
}

/** Distinguishes "we are navigating to the login page" from a real failure, so callers can decide
 * whether to stop quietly or surface an error. */
export type EnsureAuthResult = "authenticated" | "redirecting" | "failed";

/**
 * Ensure a bearer token is available before the app loads data.
 *
 * Embedded (silent): the same lazy silent acquisition getAccessToken() performs per request, just
 * eager. Standalone (interactive): if no session can be established silently, redirect the whole
 * page to the login endpoint ("redirecting" — the caller should stop, navigation has begun).
 * "failed" means neither worked and the caller should surface an error.
 */
export async function ensureAuthenticated(): Promise<EnsureAuthResult> {
  if (!client) {
    return "failed";
  }

  // getAccessToken already starts the interactive redirect on silent failure in interactive mode.
  const token = await getAccessToken();
  if (token) {
    return "authenticated";
  }

  if (authFlow === "interactive") {
    return (await signinRedirect()) ? "redirecting" : "failed";
  }

  return "failed";
}

let interactiveSigninStarted = false;

/** Resolves when the tab is visible (immediately if it already is). */
function whenVisible(): Promise<void> {
  if (document.visibilityState === "visible") {
    return Promise.resolve();
  }

  return new Promise((resolve) => {
    const onChange = () => {
      if (document.visibilityState === "visible") {
        document.removeEventListener("visibilitychange", onChange);
        resolve();
      }
    };
    document.addEventListener("visibilitychange", onChange);
  });
}

/**
 * Begin an interactive login (full-page redirect to the authorization endpoint); the round trip
 * returns to the configured `redirectUri` callback page. Guarded so concurrent failures (parallel
 * API calls all 401ing at once) start the redirect only once, and deferred until the tab is
 * visible: OpenIddict caches the authorization request server-side while the login form is shown
 * (authorize?request_uri=urn:...), and that cached entry expires — redirecting a hidden tab parks
 * the login form unattended, so by the time the user returns and submits, the request is stale and
 * OpenIddict rejects it with invalid_token ("The specified token is no longer valid").
 * Returns true when the redirect was started (or already in flight), false when it could not be.
 */
export async function signinRedirect(): Promise<boolean> {
  if (!client) {
    return false;
  }

  if (interactiveSigninStarted) {
    return true;
  }
  interactiveSigninStarted = true;

  try {
    await whenVisible();
    await client.manager.signinRedirect();
    return true;
  } catch (err) {
    // Allow a later attempt (e.g. transient network failure reaching the discovery document).
    interactiveSigninStarted = false;
    console.error(`${LOG_LABEL} interactive sign-in redirect failed`, err);
    return false;
  }
}

/**
 * Complete the interactive login. Runs on the callback page (callback.html), which re-enters this
 * module; rebuilds the UserManager from the persisted settings (the callback page has no config
 * source) and exchanges the authorization code for tokens.
 */
export async function handleRedirectCallback(): Promise<boolean> {
  try {
    const raw = window.sessionStorage.getItem(CONFIG_STORAGE_KEY);
    if (!raw) {
      return false;
    }

    const config = JSON.parse(raw) as IOidcConfig;
    return await runRedirectCallback(toSettings(config), LOG_LABEL);
  } catch (err) {
    console.error(`${LOG_LABEL} interactive sign-in callback failed`, err);
    return false;
  }
}

/**
 * Force a silent renewal (used by the API 401-retry interceptor). In interactive mode a definitive
 * renewal failure starts a fresh interactive login (see the onSilentFailure hook) instead of
 * leaving the app dead with 401s until a manual reload.
 */
export async function renewAccessToken(): Promise<string | null> {
  return client ? client.renewToken() : null;
}

/**
 * Completes the silent-renew handshake. Runs inside the hidden iframe that loaded
 * media-gallery-oidc-silent.html; rebuilds the UserManager from the persisted settings
 * and posts the result back to the parent window.
 */
export async function handleSilentRenewCallback(): Promise<void> {
  try {
    const raw = window.sessionStorage.getItem(CONFIG_STORAGE_KEY);
    if (!raw) {
      return;
    }

    const config = JSON.parse(raw) as IOidcConfig;
    await runSilentRenewCallback(toSettings(config), LOG_LABEL);
  } catch (err) {
    console.error(`${LOG_LABEL} silent renew callback failed`, err);
  }
}
