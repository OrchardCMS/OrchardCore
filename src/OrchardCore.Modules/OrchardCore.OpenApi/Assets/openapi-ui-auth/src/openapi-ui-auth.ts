import {
  runRedirectCallback,
  runSilentRenewCallback,
  SilentAuthClient,
  type SilentAuthSettings,
} from "@bloom/services/auth/silent-oidc";

/**
 * Silent OAuth2 authorization-code + PKCE bearer auth for the OpenAPI documentation UIs
 * (Swagger, Scalar).
 *
 * The UIs are only reachable by an already cookie-authenticated admin (the module's auth gate
 * redirects anonymous visitors to /admin), so we acquire a bearer access token silently
 * (prompt=none in a hidden iframe) against the same-tenant OpenID Connect server and renew it the
 * same way — no interactive "Authorize" step. That silent core (token acquisition, renewal, the
 * silent-renew and redirect callbacks) lives in @bloom/services/auth/silent-oidc and is shared
 * with the Media gallery SPA; this bundle adds the OpenAPI-specific layer.
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
 * reverse proxies resolve correctly. The callback page openapi-oidc-silent.html tags its copy of
 * this script with data-silent, so the bundle completes a callback instead of configuring the app;
 * because that static page has no server-injected config, bootstrap persists the resolved config
 * to sessionStorage (shared same-origin with the iframe) so the callback can rebuild a matching
 * UserManager.
 */

const CONFIG_STORAGE_KEY = "openapi-ui:oidc-config";
const LOG_LABEL = "[openapi-ui]";

/**
 * Set right before a visible authorization redirect and cleared once a silent sign-in succeeds.
 * If we come back from the visible flow and the silent request still can't complete, its presence
 * stops us from redirecting again (which would loop).
 */
const INTERACTIVE_ATTEMPT_KEY = "openapi-ui:interactive-signin";

/** Doc UI URL to come back to once the visible authorization flow completes. */
const RETURN_URL_KEY = "openapi-ui:return-url";

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

let client: SilentAuthClient | null = null;

/**
 * Map the OpenAPI config to the shared core's settings. Authority and the (single) redirect URI
 * are derived from window.location + the tenant path base so tenant prefixes and reverse proxies
 * resolve correctly. Tokens live in memory only — never in web storage, where an XSS could read
 * them; a page load therefore always starts with one silent sign-in, which is cheap same-origin.
 */
function toSettings(config: OpenApiAuthConfig): SilentAuthSettings {
  const origin = window.location.origin;
  const base = config.pathBase || "";
  const authority = `${origin}${base}`.replace(/\/+$/, "");
  const redirectUri = `${origin}${base}${config.silentPath}`;

  return {
    authority,
    clientId: config.clientId,
    scope: config.scope,
    redirectUri,
    silentRedirectUri: redirectUri,
    tokenStore: "memory",
  };
}

function isInteractionRequired(err: unknown): boolean {
  const code = (err as { error?: unknown } | null)?.error;
  return typeof code === "string" && INTERACTION_REQUIRED_ERRORS.has(code);
}

/**
 * Fallback for a silent sign-in that failed because the flow needs interaction: start a full-page
 * redirect to the OpenID server, remembering the doc URL to come back to. Control does not return
 * here — the callback page brings the browser back to that URL after the server completes the
 * authorization, and the reload signs in silently. Any non-interactive error (or a second failure
 * after a visible attempt, which would loop) is left alone: the shared core already logged it and
 * the doc UI simply gets no token.
 */
function startInteractiveSignin(err: unknown): void {
  if (!isInteractionRequired(err) || window.sessionStorage.getItem(INTERACTIVE_ATTEMPT_KEY)) {
    return;
  }

  window.sessionStorage.setItem(INTERACTIVE_ATTEMPT_KEY, "1");
  window.sessionStorage.setItem(RETURN_URL_KEY, window.location.href);
  void client?.manager.signinRedirect();
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

function getToken(): Promise<string | null> {
  return client ? client.getToken() : Promise.resolve(null);
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
      await client?.removeUser();
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
    if (existing && existing !== `Bearer ${client?.getTokenSync() ?? ""}`) {
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
      console.error(`${LOG_LABEL} retry with refreshed token failed`, err);
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
    const token = client?.getTokenSync();
    if (!ui || typeof ui.preauthorizeApiKey !== "function" || !token) {
      return false;
    }

    try {
      ui.preauthorizeApiKey("Bearer", token);
    } catch (err) {
      console.error(`${LOG_LABEL} preauthorize failed`, err);
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

  // Re-reflect the token after each silent renewal (the core updates its cached token first).
  client?.manager.events.addUserLoaded(() => {
    apply();
  });
}

/**
 * Completes an authorization callback on openapi-oidc-silent.html, rebuilding the UserManager from
 * the persisted config. The same page is the redirect URI for two flows:
 *  - loaded in the hidden renew iframe (window.parent !== window): the automaticSilentRenew
 *    handshake — post the result back to the parent window;
 *  - loaded top-level: the landing of the visible authorization redirect — finish it, then send the
 *    browser back to the doc UI that started the flow.
 */
async function handleCallback(): Promise<void> {
  const raw = window.sessionStorage.getItem(CONFIG_STORAGE_KEY);
  if (!raw) {
    return;
  }

  const config = JSON.parse(raw) as OpenApiAuthConfig;
  const settings = toSettings(config);

  if (window.parent !== window) {
    await runSilentRenewCallback(settings, LOG_LABEL);
    return;
  }

  // A stranded blank callback page is a dead end for the interactive flow, so return to the doc UI
  // (or the tenant root) whether or not the exchange succeeded — its silent sign-in can retry, and
  // the INTERACTIVE_ATTEMPT_KEY flag stops a persistent failure from redirecting in a loop.
  await runRedirectCallback(settings, LOG_LABEL);

  const returnUrl = window.sessionStorage.getItem(RETURN_URL_KEY);
  window.sessionStorage.removeItem(RETURN_URL_KEY);
  window.location.replace(returnUrl ?? `${window.location.origin}${config.pathBase || ""}`);
}

function bootstrap(): void {
  // document.currentScript is null in ES modules — find our own tag by its marker attribute.
  const script = document.querySelector<HTMLScriptElement>("script[data-openapi-ui-auth]");
  if (!script) {
    return;
  }

  // On the callback page (openapi-oidc-silent.html tags its copy of this script with data-silent):
  // complete the handshake and do not configure the app.
  if (script.hasAttribute("data-silent")) {
    void handleCallback();
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

  // Persist so the config-less callback page can rebuild a matching UserManager.
  window.sessionStorage.setItem(CONFIG_STORAGE_KEY, JSON.stringify(config));

  client = new SilentAuthClient(toSettings(config), {
    label: LOG_LABEL,
    // A silent failure that needs interaction (no OpenID session yet, or consent not recorded)
    // falls back to a visible authorization flow rather than leaving the doc UI without a token.
    onSilentFailure: startInteractiveSignin,
  });

  // A silent sign-in succeeded, so a later interaction-required failure is a genuinely new one and
  // may start its own visible flow.
  client.manager.events.addUserLoaded(() => {
    window.sessionStorage.removeItem(INTERACTIVE_ATTEMPT_KEY);
  });

  installFetch();
  preauthorizeSwagger();

  // Eagerly acquire a token so the first "try it out" request is instant. Module scripts are
  // deferred, so document.body (where oidc-client-ts appends its hidden iframe) already exists.
  void getToken();
}

bootstrap();
