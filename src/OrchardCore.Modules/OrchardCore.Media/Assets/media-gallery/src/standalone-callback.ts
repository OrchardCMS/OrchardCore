import { handleRedirectCallback } from "./services/auth";

/**
 * Interactive-login callback (callback.html). oidc-client-ts persists the auth state (code verifier,
 * etc.) in sessionStorage under the app origin, which survives the cross-origin round trip to the
 * IdP and back, so this page can complete the authorization-code exchange and then return the tab to
 * the app root.
 */
async function run(): Promise<void> {
  await handleRedirectCallback();
  window.location.replace("./");
}

void run();
