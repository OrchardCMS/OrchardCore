import { UserManager, WebStorageStateStore, type UserManagerSettings } from "oidc-client-ts";

/**
 * OAuth2 authorization-code + PKCE authentication for the media gallery SPA.
 *
 * The admin user is already cookie-authenticated, so we acquire a bearer access
 * token silently (prompt=none in a hidden iframe) via oidc-client-ts and renew it
 * the same way. The token is then attached to every Media API / Uppy / SignalR call.
 *
 * The silent-renew iframe loads media-gallery-oidc-silent.html, which re-enters this
 * module (see main.ts) and calls handleSilentRenewCallback(). Because that page has no
 * <media-gallery> element to read config from, configureAuth() persists the resolved
 * settings to sessionStorage so the callback can rebuild a matching UserManager.
 */

const CONFIG_STORAGE_KEY = "media-gallery:oidc-settings";

export interface IOidcConfig {
  /** Absolute issuer URL (tenant root); discovery is at {authority}/.well-known/openid-configuration. */
  authority: string;
  clientId: string;
  scope: string;
  /** Redirect URI used for the (unused) interactive flow; kept equal to the silent URI. */
  redirectUri: string;
  /** Silent-renew redirect URI — must match a RedirectUris entry registered for the client. */
  silentRedirectUri: string;
}

let userManager: UserManager | null = null;
let cachedToken: string | null = null;

function buildSettings(config: IOidcConfig): UserManagerSettings {
  return {
    authority: config.authority,
    client_id: config.clientId,
    redirect_uri: config.redirectUri,
    silent_redirect_uri: config.silentRedirectUri,
    scope: config.scope,
    response_type: "code",
    automaticSilentRenew: true,
    // First-party admin app; no need to monitor the OP session via the check_session iframe.
    monitorSession: false,
    userStore: new WebStorageStateStore({ store: window.sessionStorage }),
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

  userManager = new UserManager(buildSettings(config));
  userManager.events.addUserLoaded((user) => {
    cachedToken = user.access_token ?? null;
  });
  userManager.events.addUserUnloaded(() => {
    cachedToken = null;
  });

  return true;
}

export function isAuthConfigured(): boolean {
  return userManager !== null;
}

/**
 * Return a valid access token, silently acquiring/renewing one if needed.
 * Used by the axios request interceptor.
 */
export async function getAccessToken(): Promise<string | null> {
  if (!userManager) {
    return null;
  }

  try {
    let user = await userManager.getUser();
    if (!user || user.expired) {
      user = await userManager.signinSilent();
    }
    cachedToken = user?.access_token ?? null;
    return cachedToken;
  } catch (err) {
    console.error("[media-gallery] silent sign-in failed", err);
    cachedToken = null;
    return null;
  }
}

/**
 * Synchronous accessor for the last-acquired token. Used where an async provider is not
 * accepted (e.g. the SignalR access-token factory). May be empty before the first
 * getAccessToken() resolves.
 */
export function getAccessTokenSync(): string {
  return cachedToken ?? "";
}

/**
 * Force a silent renewal (used by the API 401-retry interceptor).
 */
export async function renewAccessToken(): Promise<string | null> {
  if (!userManager) {
    return null;
  }

  try {
    const user = await userManager.signinSilent();
    cachedToken = user?.access_token ?? null;
    return cachedToken;
  } catch (err) {
    console.error("[media-gallery] token renewal failed", err);
    return null;
  }
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
    const manager = new UserManager(buildSettings(config));
    await manager.signinSilentCallback();
  } catch (err) {
    console.error("[media-gallery] silent renew callback failed", err);
  }
}
