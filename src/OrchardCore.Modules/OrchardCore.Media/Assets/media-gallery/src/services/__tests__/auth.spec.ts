import { describe, it, expect, vi, beforeEach } from "vitest";

// Capture every UserManager the module constructs so tests can drive/assert its methods.
const instances: Array<Record<string, ReturnType<typeof vi.fn>>> = [];

vi.mock("oidc-client-ts", () => {
  return {
    WebStorageStateStore: vi.fn(),
    UserManager: vi.fn().mockImplementation(() => {
      const inst = {
        getUser: vi.fn().mockResolvedValue(null),
        signinSilent: vi.fn().mockResolvedValue(null),
        signinRedirect: vi.fn().mockResolvedValue(undefined),
        signinRedirectCallback: vi.fn().mockResolvedValue(undefined),
        signinSilentCallback: vi.fn().mockResolvedValue(undefined),
        events: { addUserLoaded: vi.fn(), addUserUnloaded: vi.fn() },
      };
      instances.push(inst);
      return inst;
    }),
  };
});

import { configureAuth, ensureAuthenticated, isAuthConfigured } from "../auth";

const lastInstance = () => instances[instances.length - 1];

const baseConfig = {
  authority: "https://cms.example.com",
  clientId: "media_gallery",
  scope: "openid roles",
  redirectUri: "https://app.example.com/callback.html",
  silentRedirectUri: "https://app.example.com/silent-callback.html",
};

beforeEach(() => {
  instances.length = 0;
});

describe("configureAuth", () => {
  it("returns false and configures nothing without authority/clientId", () => {
    expect(configureAuth({ ...baseConfig, authority: "" })).toBe(false);
    expect(instances.length).toBe(0);
  });

  it("returns true and marks auth configured with a valid config", () => {
    expect(configureAuth({ ...baseConfig })).toBe(true);
    expect(isAuthConfigured()).toBe(true);
  });
});

describe("ensureAuthenticated", () => {
  it("returns true when a valid (non-expired) user already exists, without redirecting", async () => {
    configureAuth({ ...baseConfig });
    lastInstance().getUser.mockResolvedValue({ access_token: "tok", expired: false });

    expect(await ensureAuthenticated()).toBe(true);
    expect(lastInstance().signinRedirect).not.toHaveBeenCalled();
  });

  it("acquires a token via silent sign-in when there is no current user", async () => {
    configureAuth({ ...baseConfig, authFlow: "interactive" });
    lastInstance().getUser.mockResolvedValue(null);
    lastInstance().signinSilent.mockResolvedValue({ access_token: "tok2", expired: false });

    expect(await ensureAuthenticated()).toBe(true);
    expect(lastInstance().signinRedirect).not.toHaveBeenCalled();
  });

  it("does NOT redirect in silent (embedded) mode when no session can be established", async () => {
    configureAuth({ ...baseConfig, authFlow: "silent" });
    lastInstance().getUser.mockResolvedValue(null);
    lastInstance().signinSilent.mockResolvedValue(null);

    expect(await ensureAuthenticated()).toBe(false);
    expect(lastInstance().signinRedirect).not.toHaveBeenCalled();
  });

  it("redirects to the interactive login when silent fails in interactive (standalone) mode", async () => {
    configureAuth({ ...baseConfig, authFlow: "interactive" });
    lastInstance().getUser.mockResolvedValue(null);
    lastInstance().signinSilent.mockRejectedValue(new Error("login_required"));

    expect(await ensureAuthenticated()).toBe(false);
    expect(lastInstance().signinRedirect).toHaveBeenCalledOnce();
  });
});
