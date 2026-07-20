/**
 * Runtime configuration for the media gallery, resolved from one of two sources and consumed by
 * every service that needs a URL, an origin, or an auth setting. It makes the **origin split**
 * explicit so the same app can run in two topologies:
 *
 *  - **Embedded** (default): served by Orchard as the `<media-gallery>` custom element. Config comes
 *    from the element's attributes (see App.vue props). Both origins collapse to the current origin,
 *    so behavior is identical to before this abstraction existed.
 *  - **Standalone**: hosted on its own origin (static host / CDN) against a remote Orchard tenant.
 *    Config comes from a fetched `config.json` (see resolveStandaloneConfig). The **app origin**
 *    (HTML + OIDC callback pages, i.e. the redirect URIs) is distinct from the **Orchard origin**
 *    (issuer, API, SignalR hub, media files).
 *
 * Nothing downstream should read `window.location` directly — read the resolved config instead.
 */

export type ApiAuthScheme = "Cookie" | "Bearer";

/** How the bearer token is first acquired. Silent needs an existing Orchard admin session on the
 * Orchard origin (embedded); interactive redirects to the login page (standalone). */
export type AuthFlow = "silent" | "interactive";

export interface IMediaRuntimeConfig {
  /** Origin+base that hosts the SPA and its OIDC callback pages (the OIDC redirect URIs live here). */
  appBaseUrl: string;
  /** Origin+base of the Orchard tenant: issuer, API, SignalR hub, media files. */
  orchardBaseUrl: string;
  /** Base URL handed to the NSwag `MediaApiClient` (it appends "api/media/..."). Relative in embedded
   * mode (e.g. "/"), absolute in standalone mode. */
  apiBaseUrl: string;
  /** URL of the MediaHub (relative same-origin in embedded, absolute in standalone). */
  hubUrl: string;
  /** OIDC issuer/authority (discovery at `{authority}/.well-known/openid-configuration`). */
  authority: string;
  clientId: string;
  scope: string;
  /** OIDC redirect URI (on the app origin). */
  redirectUri: string;
  /** OIDC silent-renew redirect URI (on the app origin); must match a registered client RedirectUri. */
  silentRedirectUri: string;
  /** Which auth scheme the Media API expects (server-chosen via MediaApiSettings). */
  apiAuthScheme: ApiAuthScheme;
  /** Token-acquisition flow. */
  authFlow: AuthFlow;
}

/** The `<media-gallery>` attributes App.vue receives, as far as config resolution cares. */
export interface IEmbeddedConfigSource {
  basePath: string;
  oidcAuthority?: string;
  oidcClientId?: string;
  oidcScope?: string;
  apiAuthScheme?: string;
}

/** The shape of a standalone `config.json`. Only `orchardBaseUrl` is required. */
export interface IStandaloneConfigSource {
  /** Absolute origin+base of the remote Orchard tenant (include any tenant prefix), e.g.
   * "https://cms.example.com/" or "https://example.com/team-a/". */
  orchardBaseUrl: string;
  /** The app's own origin+base; defaults to the current page origin+path. */
  appBaseUrl?: string;
  oidcAuthority?: string;
  oidcClientId?: string;
  oidcScope?: string;
  /** Enable real-time updates. Requires SignalR CORS on the Orchard origin; defaults to false. */
  signalrEnabled?: boolean;
}

const DEFAULT_CLIENT_ID = "media_gallery";
const DEFAULT_SCOPE = "openid email profile roles";
/** Conventional silent-renew page, resolved relative to the app origin. */
const SILENT_RENEW_PAGE = "OrchardCore.Media/media-gallery-oidc-silent.html";

function withTrailingSlash(value: string): string {
  return value.endsWith("/") ? value : `${value}/`;
}

function stripTrailingSlash(value: string): string {
  return value.endsWith("/") ? value.slice(0, -1) : value;
}

/**
 * Resolve config for the **embedded** case from the `<media-gallery>` attributes. Both origins
 * collapse to the current origin, and the API base + hub URL keep their exact pre-refactor values
 * (relative `basePath` and "/hubs/media") so embedded behavior is unchanged.
 */
export function resolveEmbeddedConfig(source: IEmbeddedConfigSource): IMediaRuntimeConfig {
  const origin = window.location.origin;
  const base = withTrailingSlash(source.basePath);
  const appBaseUrl = `${origin}${base}`;
  const silentUri = `${appBaseUrl}${SILENT_RENEW_PAGE}`;

  return {
    appBaseUrl,
    orchardBaseUrl: appBaseUrl,
    // Keep the relative base the MediaApiClient has always received.
    apiBaseUrl: source.basePath,
    // Keep the exact same-origin hub path used before the origin split.
    hubUrl: "/hubs/media",
    authority: source.oidcAuthority || stripTrailingSlash(appBaseUrl),
    clientId: source.oidcClientId || DEFAULT_CLIENT_ID,
    scope: source.oidcScope || DEFAULT_SCOPE,
    redirectUri: silentUri,
    silentRedirectUri: silentUri,
    apiAuthScheme: source.apiAuthScheme === "Bearer" ? "Bearer" : "Cookie",
    authFlow: "silent",
  };
}

/**
 * Resolve config for the **standalone** case from a fetched `config.json`. The app origin (redirect
 * URIs) is separated from the Orchard origin (issuer/API/hub). Standalone is always bearer + an
 * interactive login (there is no ambient Orchard cookie on the app origin).
 */
export function resolveStandaloneConfig(source: IStandaloneConfigSource): IMediaRuntimeConfig {
  const appBaseUrl = withTrailingSlash(source.appBaseUrl || `${window.location.origin}${window.location.pathname}`);
  const orchardBaseUrl = withTrailingSlash(source.orchardBaseUrl);
  const silentUri = `${appBaseUrl}silent-callback.html`;
  const redirectUri = `${appBaseUrl}callback.html`;

  return {
    appBaseUrl,
    orchardBaseUrl,
    apiBaseUrl: orchardBaseUrl,
    hubUrl: `${orchardBaseUrl}hubs/media`,
    authority: source.oidcAuthority || stripTrailingSlash(orchardBaseUrl),
    clientId: source.oidcClientId || DEFAULT_CLIENT_ID,
    scope: source.oidcScope || DEFAULT_SCOPE,
    redirectUri,
    silentRedirectUri: silentUri,
    apiAuthScheme: "Bearer",
    authFlow: "interactive",
  };
}

let current: IMediaRuntimeConfig | null = null;

/** Store the resolved config for services to read via useRuntimeConfig(). */
export function setRuntimeConfig(config: IMediaRuntimeConfig): void {
  current = config;
}

/**
 * Read the resolved runtime config. Falls back to an embedded resolution from a root basePath if
 * setRuntimeConfig() has not run yet (e.g. a service instantiated before App.vue setup) so callers
 * never see null.
 */
export function useRuntimeConfig(): IMediaRuntimeConfig {
  if (!current) {
    current = resolveEmbeddedConfig({ basePath: "/" });
  }
  return current;
}
