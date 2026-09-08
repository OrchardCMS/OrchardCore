import {
    InMemoryWebStorage,
    UserManager,
    WebStorageStateStore,
    type UserManagerSettings,
} from "oidc-client-ts";

/**
 * Shared silent OAuth2 authorization-code + PKCE core for Orchard Core first-party admin SPAs
 * (the Media gallery, the OpenAPI documentation UIs, …).
 *
 * Every such consumer authenticates the same way: the admin is already cookie-authenticated, so a
 * bearer access token is acquired silently (prompt=none in a hidden iframe) against the same-tenant
 * OpenID Connect server and renewed the same way. This module owns exactly that common surface —
 * the UserManager settings block, silent acquire/renew, the token cache, and the silent-renew
 * callback. Consumer-specific concerns stay in the consumer:
 *
 *  - how config is sourced (element attributes, script data-* attributes, fetched config.json)
 *    and mapped to {@link SilentAuthSettings};
 *  - how the token is attached to requests (axios interceptor, window.fetch wrapper, SignalR
 *    factory);
 *  - any interactive full-page login flow (silent-only consumers have none).
 */

export type TokenStore = "session" | "memory";

/** Normalized inputs for a silent-PKCE UserManager, resolved by each consumer from its own config
 * source. Absolute URLs so tenant prefixes and reverse proxies are already accounted for. */
export interface SilentAuthSettings {
    /** Absolute issuer URL (tenant root); discovery is at {authority}/.well-known/openid-configuration. */
    authority: string;
    clientId: string;
    scope: string;
    /** Redirect URI for the primary flow (equals the silent URI in embedded/silent-only mode). */
    redirectUri: string;
    /** Silent-renew redirect URI — must match a RedirectUris entry registered for the client. */
    silentRedirectUri: string;
    /**
     * Where oidc-client-ts persists the User (i.e. the tokens). `session` survives reloads within
     * the tab; `memory` keeps tokens out of web storage entirely so an XSS cannot read them, at the
     * cost of one silent sign-in per page load (cheap, same-origin). Defaults to `session`.
     *
     * Only the User store is affected — the transient signin *state* (PKCE verifier, no tokens)
     * always uses the default sessionStorage state store, which the silent-renew iframe handshake
     * needs and which is cleared after each flow.
     */
    tokenStore?: TokenStore;
}

/**
 * Build the UserManagerSettings shared by every first-party silent-PKCE consumer: authorization
 * code + PKCE, automatic silent renew, and no OP session monitoring (first-party admin app — no
 * need for the check_session iframe).
 */
export function buildSilentUserManagerSettings(settings: SilentAuthSettings): UserManagerSettings {
    const store =
        settings.tokenStore === "memory" ? new InMemoryWebStorage() : window.sessionStorage;

    return {
        authority: settings.authority,
        client_id: settings.clientId,
        redirect_uri: settings.redirectUri,
        silent_redirect_uri: settings.silentRedirectUri,
        scope: settings.scope,
        response_type: "code",
        automaticSilentRenew: true,
        monitorSession: false,
        userStore: new WebStorageStateStore({ store }),
    };
}

export interface SilentAuthClientOptions {
    /** Log prefix for diagnostics, e.g. "[media-gallery]" or "[openapi-ui]". */
    label: string;
    /**
     * Invoked (after logging) when a silent acquisition or renewal definitively fails. Lets an
     * interactive consumer start a full-page login instead of leaving the app dead with 401s until
     * a manual reload; silent-only consumers omit it and simply get a null token.
     */
    onSilentFailure?: (err: unknown) => void;
}

/**
 * Wraps a UserManager with a cached-token layer over silent acquisition and renewal. One instance
 * per configured app; consumers read the token via {@link getToken} (async, for request
 * interceptors) or {@link getTokenSync} (for APIs that cannot await, e.g. the SignalR access-token
 * factory).
 */
export class SilentAuthClient {
    private readonly userManager: UserManager;
    private readonly label: string;
    private readonly onSilentFailure?: (err: unknown) => void;
    private cachedToken: string | null = null;

    constructor(settings: SilentAuthSettings, options: SilentAuthClientOptions) {
        this.label = options.label;
        this.onSilentFailure = options.onSilentFailure;
        this.userManager = new UserManager(buildSilentUserManagerSettings(settings));
        this.userManager.events.addUserLoaded((user) => {
            this.cachedToken = user.access_token ?? null;
        });
        this.userManager.events.addUserUnloaded(() => {
            this.cachedToken = null;
        });
    }

    /** The underlying UserManager, for consumer-specific wiring (e.g. re-reflecting the token in a
     * UI after each silent renewal). */
    get manager(): UserManager {
        return this.userManager;
    }

    /**
     * Synchronous accessor for the last-acquired token. Used where an async provider is not accepted
     * (e.g. the SignalR access-token factory). Empty string before the first {@link getToken}
     * resolves.
     */
    getTokenSync(): string {
        return this.cachedToken ?? "";
    }

    /** Return a valid access token, silently acquiring or renewing one if needed. Returns null on a
     * definitive silent failure (after invoking onSilentFailure). */
    async getToken(): Promise<string | null> {
        try {
            let user = await this.userManager.getUser();
            if (!user || user.expired) {
                user = await this.userManager.signinSilent();
            }
            this.cachedToken = user?.access_token ?? null;
            return this.cachedToken;
        } catch (err) {
            console.error(`${this.label} silent sign-in failed`, err);
            this.cachedToken = null;
            this.onSilentFailure?.(err);
            return null;
        }
    }

    /** Force a silent renewal, bypassing any cached user. Returns the new token or null. */
    async renewToken(): Promise<string | null> {
        try {
            const user = await this.userManager.signinSilent();
            this.cachedToken = user?.access_token ?? null;
            return this.cachedToken;
        } catch (err) {
            console.error(`${this.label} token renewal failed`, err);
            this.onSilentFailure?.(err);
            return null;
        }
    }

    /** Discard the persisted user (and cached token) so the next {@link getToken} signs in afresh.
     * Needed when the server rejects a token that is not yet expired by the clock. */
    async removeUser(): Promise<void> {
        await this.userManager.removeUser();
        this.cachedToken = null;
    }
}

/**
 * Complete a silent-renew handshake inside the hidden iframe. The iframe page has no app config
 * source, so the consumer rebuilds the settings (from config it persisted to sessionStorage before
 * the flow) and hands them here; this rebuilds a matching UserManager and posts the result back to
 * the parent window.
 */
export async function runSilentRenewCallback(
    settings: SilentAuthSettings,
    label: string,
): Promise<void> {
    try {
        const manager = new UserManager(buildSilentUserManagerSettings(settings));
        await manager.signinSilentCallback();
    } catch (err) {
        console.error(`${label} silent renew callback failed`, err);
    }
}

/**
 * Complete an interactive authorization-code redirect on the callback page — the full-page
 * counterpart of {@link runSilentRenewCallback}. Rebuilds a UserManager from the persisted settings
 * (the callback page has no app config source) and exchanges the authorization code for tokens.
 *
 * Returns false (rather than throwing) on failure so the callback page can surface the error
 * instead of bouncing back into the app, which would immediately redirect to the IdP again — an
 * infinite authorize/callback loop when the failure is persistent (e.g. access_denied, lost state).
 */
export async function runRedirectCallback(
    settings: SilentAuthSettings,
    label: string,
): Promise<boolean> {
    try {
        const manager = new UserManager(buildSilentUserManagerSettings(settings));
        await manager.signinRedirectCallback();
        return true;
    } catch (err) {
        console.error(`${label} interactive sign-in callback failed`, err);
        return false;
    }
}
