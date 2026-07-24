import {
  InMemoryWebStorage,
  UserManager,
  WebStorageStateStore,
  type User,
  type UserManagerSettings,
} from "oidc-client-ts";

/**
 * Silent OAuth2 authorization-code + PKCE bearer auth for the OpenAPI documentation UIs
 * (Swagger, Scalar).
 *
 * The UIs are only reachable by an already cookie-authenticated admin (the module's auth gate
 * redirects anonymous visitors to /admin), so we acquire a bearer access token silently
 * (prompt=none in a hidden iframe) against the same-tenant OpenID Connect server and renew it the
 * same way — no interactive "Authorize" step.
 *
 * If the silent (prompt=none) request comes back with a standard interaction-required OIDC error —
 * the admin has no OpenID session yet, or consent hasn't been granted because the client uses a
 * non-implicit consent type (explicit/systematic) — the bundle falls back to a real, visible
 * authorization flow (a full-page redirect). The OpenID server establishes the session / records
 * consent and redirects back through the silent-renew page, which returns the browser to the doc
 * UI; the reload's silent sign-in then succeeds. A sessionStorage flag guards against a redirect
 * loop when the silent flow still can't complete after a visible attempt.
 *
 * The bundle is a self-bootstrapping ES module: the server injects a single
 * <script type="module" src="…" data-openapi-ui-auth data-client-id="…" …> tag (see
 * Startup.BuildAuthHeadContent) and everything else happens here on load — no inline scripts, no
 * window globals. Config is read from that tag's data-* attributes (document.currentScript is
 * null inside modules, so the tag is found via its data-openapi-ui-auth marker). On boot the
 * bundle wraps window.fetch so every "try it out" API request carries the token — retrying once
 * with a fresh token when the server rejects the cached one (e.g. a token held by a long-lived
 * tab across a site re-setup can no longer be unprotected server-side) — and reflects the token
 * in Swagger's Authorize dialog when Swagger UI is present.
 *
 * The authority and redirect URI are derived here from window.location so tenant prefixes and
 * reverse proxies resolve correctly. The silent-renew iframe loads openapi-oidc-silent.html,
 * whose copy of this script tag carries data-silent, so the bundle completes the handshake
 * instead of configuring the app; because that static page has no server-injected config,
 * bootstrap persists the resolved config to sessionStorage (shared same-origin with the iframe)
 * so the callback can rebuild a matching UserManager.
 */

const CONFIG_STORAGE_KEY = "openapi-ui:oidc-config";

/**
 * Set right before a visible authorization redirect and cleared once a silent sign-in succeeds.
 * If we come back from the visible flow and the silent request still can't complete, its presence
 * stops us from redirecting again (which would loop).
 */
const INTERACTIVE_ATTEMPT_KEY = "openapi-ui:interactive-signin";

/**
 * Standard OAuth 2.0 / OpenID Connect error codes a prompt=none authorization request returns when
 * the user cannot be signed in without interaction. On any of these we start a visible flow rather
 * than failing; anything else (a real configuration/transport error) is surfaced as-is.
 */
const INTERACTION_REQUIRED_ERRORS = new Set([
  "login_required",
  "consent_required",
  "interaction_required",
  "account_selection_required",
]);

interface OpenApiAuthConfig {
  /** Tenant path base (e.g. "" for the Default tenant, "/team1" for a prefixed tenant). */
  pathBase: string;
  clientId: string;
  scope: string;
  /** Module-relative path of the silent-renew callback page. */
  silentPath: string;
}

/** The subset of the Swagger UI system object we use to pre-fill the Authorize dialog. */
interface SwaggerUISystem {
  preauthorizeApiKey?: (authDefinitionKey: string, value: string) => void;
}

let userManager: UserManager | null = null;
let cachedToken: string | null = null;

function buildSettings(config: OpenApiAuthConfig): UserManagerSettings {
  const origin = window.location.origin;
  const base = config.pathBase || "";
  const authority = `${origin}${base}`.replace(/\/+$/, "");
  const redirectUri = `${origin}${base}${config.silentPath}`;

  return {
    authority,
    client_id: config.clientId,
    redirect_uri: redirectUri,
    silent_redirect_uri: redirectUri,
    scope: config.scope,
    response_type: "code",
    automaticSilentRenew: true,
    // First-party admin app; no need to monitor the OP session via the check_session iframe.
    monitorSession: false,
    // Tokens live in memory only — never in web storage, where an XSS could read them. A page
    // load therefore always starts with one silent sign-in, which is cheap same-origin. (The
    // transient signin *state* — PKCE verifier, no tokens — still uses the default sessionStorage
    // stateStore: the silent-renew iframe handshake needs it, and it is removed after each flow.)
    userStore: new WebStorageStateStore({ store: new InMemoryWebStorage() }),
  };
}

/**
 * Only real API calls carry the bearer token. The spec (/swagger/*.json, /openapi*), the UI assets
 * (/swagger/*, /scalar/*), the OIDC token/discovery requests (/connect/*, /.well-known/*) must NOT:
 * spec/UI fetches keep the admin cookie so the auth gate does not redirect them to /admin, and the
 * OIDC requests are issued by oidc-client-ts itself. OrchardCore API endpoints live under "/api/".
 * Same-origin only: Scalar lets the target server be edited (and a spec can list external
 * servers), and the token must never be sent to a foreign host.
 */
function isApiRequest(url: string | undefined): boolean {
  if (!url) {
    return false;
  }

  try {
    const resolved = new URL(url, window.location.href);
    return resolved.origin === window.location.origin && resolved.pathname.includes("/api/");
  } catch {
    return false;
  }
}

function isInteractionRequired(err: unknown): boolean {
  const code = (err as { error?: unknown } | null)?.error;
  return typeof code === "string" && INTERACTION_REQUIRED_ERRORS.has(code);
}

/**
 * Silent sign-in, with a visible authorization flow as the fallback. When prompt=none fails with a
 * standard interaction-required error, we start a full-page redirect to the OpenID server carrying
 * the current doc URL as the returned state; control does not come back here — the silent-renew
 * page brings the browser back to that URL after the server completes the authorization, and the
 * reload signs in silently. Any non-interactive error is rethrown.
 */
async function signinSilentOrInteractive(): Promise<User | null> {
  try {
    const user = await userManager!.signinSilent();
    window.sessionStorage.removeItem(INTERACTIVE_ATTEMPT_KEY);
    return user;
  } catch (err) {
    // Already tried the visible flow this session and silent still can't complete: surface the
    // error instead of redirecting again, which would loop.
    if (!isInteractionRequired(err) || window.sessionStorage.getItem(INTERACTIVE_ATTEMPT_KEY)) {
      throw err;
    }

    window.sessionStorage.setItem(INTERACTIVE_ATTEMPT_KEY, "1");
    await userManager!.signinRedirect({ state: { returnUrl: window.location.href } });
    return null;
  }
}

async function getToken(): Promise<string | null> {
  if (!userManager) {
    return null;
  }

  try {
    let user = await userManager.getUser();
    if (!user || user.expired) {
      user = await signinSilentOrInteractive();
    }
    cachedToken = user?.access_token ?? null;
  } catch (err) {
    console.error("[openapi-ui] sign-in failed", err);
    cachedToken = null;
  }

  return cachedToken;
}

let refreshInFlight: Promise<string | null> | null = null;

/**
 * Discards the cached user and silently signs in again. Needed when the server rejects a token
 * that is not yet expired by the clock: getToken() would keep returning it until expiry (e.g. a
 * token a long-lived tab acquired before a site re-setup, which the new Data Protection keys can
 * no longer unprotect). Concurrent callers (Scalar fires several API requests at once) share a
 * single sign-in.
 */
function refreshToken(): Promise<string | null> {
  refreshInFlight ??= (async () => {
    try {
      await userManager?.removeUser();
    } finally {
      refreshInFlight = null;
    }
    return getToken();
  })();

  return refreshInFlight;
}

function installFetch(): void {
  const originalFetch = window.fetch.bind(window);

  window.fetch = async (input: RequestInfo | URL, init?: RequestInit): Promise<Response> => {
    const url =
      typeof input === "string" ? input : input instanceof URL ? input.href : input.url;

    if (!isApiRequest(url)) {
      return originalFetch(input, init);
    }

    // A caller-provided token (e.g. one pasted into Swagger's Authorize dialog) that is not the
    // one we acquired is respected untouched — no override, no retry on its behalf.
    const existing = new Headers(init?.headers).get("Authorization");
    if (existing && existing !== `Bearer ${cachedToken}`) {
      return originalFetch(input, init);
    }

    // Cloned before the body is consumed so a retry can resend Request-object inputs.
    const retryInput = input instanceof Request ? input.clone() : input;

    const token = await getToken();
    const headers = new Headers(init?.headers);
    if (token) {
      headers.set("Authorization", `Bearer ${token}`);
    }

    const response = await originalFetch(input, { ...init, headers, credentials: "omit" });
    if (response.status !== 401 || !token) {
      return response;
    }

    const fresh = await refreshToken();
    if (!fresh || fresh === token) {
      return response;
    }

    try {
      headers.set("Authorization", `Bearer ${fresh}`);
      return await originalFetch(retryInput, { ...init, headers, credentials: "omit" });
    } catch (err) {
      console.error("[openapi-ui] retry with refreshed token failed", err);
      return response;
    }
  };
}

/**
 * Reflects the silently-acquired token in Swagger UI's Authorize dialog so it shows as already
 * authorized (green) with no user action. Purely cosmetic — the request interceptor is what
 * actually attaches (and refreshes) the token on each call. Polls for window.ui because Swagger
 * UI initializes after this bundle loads, and re-applies after every silent renewal. Runs on
 * every page the bundle boots on; where Swagger UI never appears (Scalar), the poll simply times
 * out below.
 */
function preauthorizeSwagger(): void {
  const apply = (): boolean => {
    const ui = (window as unknown as { ui?: SwaggerUISystem }).ui;
    if (!ui || typeof ui.preauthorizeApiKey !== "function" || !cachedToken) {
      return false;
    }

    try {
      ui.preauthorizeApiKey("Bearer", cachedToken);
    } catch (err) {
      console.error("[openapi-ui] preauthorize failed", err);
    }

    // Applied (or hard-failed) — either way stop polling.
    return true;
  };

  if (!apply()) {
    const timer = window.setInterval(() => {
      if (apply()) {
        window.clearInterval(timer);
      }
    }, 500);

    // Give up if window.ui never appears (not the Swagger page) or no token ever arrives
    // (e.g. the client is not registered), so we don't loop forever.
    window.setTimeout(() => window.clearInterval(timer), 30000);
  }

  // Re-reflect the token after each silent renewal.
  userManager?.events.addUserLoaded((user) => {
    cachedToken = user.access_token ?? null;
    apply();
  });
}

/**
 * Completes an authorization callback on openapi-oidc-silent.html, rebuilding the UserManager from
 * the persisted config. The same page is the redirect URI for two flows:
 *  - loaded in the hidden renew iframe (window.parent !== window): the automaticSilentRenew
 *    handshake — post the result back to the parent window;
 *  - loaded top-level: the landing of the visible authorization redirect — finish it, then send the
 *    browser back to the doc UI that started the flow (the returnUrl carried through OIDC state).
 */
async function handleSilentRenewCallback(): Promise<void> {
  const raw = window.sessionStorage.getItem(CONFIG_STORAGE_KEY);
  if (!raw) {
    return;
  }

  const config = JSON.parse(raw) as OpenApiAuthConfig;
  const manager = new UserManager(buildSettings(config));
  const topLevel = window.parent === window;

  try {
    if (topLevel) {
      const user = await manager.signinRedirectCallback();
      const returnUrl = (user?.state as { returnUrl?: string } | null)?.returnUrl;
      window.location.replace(returnUrl ?? `${window.location.origin}${config.pathBase || ""}`);
      return;
    }

    await manager.signinSilentCallback();
  } catch (err) {
    console.error("[openapi-ui] renew callback failed", err);
    // A stranded blank silent page is a dead end for the interactive flow; return to the origin so
    // the doc UI (and its silent sign-in) can retry rather than leaving the admin on a blank page.
    if (topLevel) {
      window.location.replace(`${window.location.origin}${config.pathBase || ""}`);
    }
  }
}

function bootstrap(): void {
  // document.currentScript is null in ES modules — find our own tag by its marker attribute.
  const script = document.querySelector<HTMLScriptElement>("script[data-openapi-ui-auth]");
  if (!script) {
    return;
  }

  // Inside the hidden silent-renew iframe (openapi-oidc-silent.html tags its copy of this script
  // with data-silent): complete the handshake and do not configure the app.
  if (script.hasAttribute("data-silent")) {
    void handleSilentRenewCallback();
    return;
  }

  const { pathBase, clientId, scope, silentPath } = script.dataset;
  if (!clientId || !silentPath) {
    return;
  }

  const config: OpenApiAuthConfig = {
    pathBase: pathBase ?? "",
    clientId,
    scope: scope ?? "",
    silentPath,
  };

  // Persist so the config-less silent-renew page can rebuild a matching UserManager.
  window.sessionStorage.setItem(CONFIG_STORAGE_KEY, JSON.stringify(config));

  userManager = new UserManager(buildSettings(config));
  userManager.events.addUserLoaded((user) => {
    cachedToken = user.access_token ?? null;
  });
  userManager.events.addUserUnloaded(() => {
    cachedToken = null;
  });

  installFetch();
  preauthorizeSwagger();

  // Eagerly acquire a token so the first "try it out" request is instant. Module scripts are
  // deferred, so document.body (where oidc-client-ts appends its hidden iframe) already exists.
  void getToken();
}

bootstrap();
