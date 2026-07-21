import { handleRedirectCallback } from "./services/media-gallery-auth";

/**
 * Interactive-login callback (callback.html). oidc-client-ts persists the auth state (code verifier,
 * etc.) in sessionStorage under the app origin, which survives the cross-origin round trip to the
 * IdP and back, so this page can complete the authorization-code exchange and then return the tab to
 * the app root.
 *
 * On failure (access_denied, lost state, direct navigation to this page) it shows the error and a
 * manual link instead of returning to the app — returning would immediately redirect back to the
 * IdP, producing an infinite authorize/callback loop when the failure is persistent.
 */
async function run(): Promise<void> {
  const succeeded = await handleRedirectCallback();
  if (succeeded) {
    window.location.replace("./");
    return;
  }

  document.body.textContent = "";
  const message = document.createElement("p");
  message.textContent = "Sign-in failed. The identity provider rejected the request or the sign-in state was lost (see the browser console for details).";
  const retry = document.createElement("a");
  retry.href = "./";
  retry.textContent = "Return to the media gallery and try again";
  document.body.append(message, retry);
}

void run();
