import { handleSilentRenewCallback } from "./services/auth";

/**
 * Silent-renew callback (silent-callback.html). Loaded inside the hidden iframe oidc-client-ts
 * creates for automatic token renewal; completes the silent handshake against the persisted config.
 */
void handleSilentRenewCallback();
