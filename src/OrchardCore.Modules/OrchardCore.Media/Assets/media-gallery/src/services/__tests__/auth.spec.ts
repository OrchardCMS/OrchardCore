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

// The auth module holds state (userManager, cachedToken, the interactive-redirect guard), so each
// test gets a fresh module instance.
let auth: typeof import("../auth");

const lastInstance = () => instances[instances.length - 1];

const baseConfig = {
  authority: "https://cms.example.com",
  clientId: "media_gallery",
  scope: "openid roles",
  redirectUri: "https://app.example.com/callback.html",
  silentRedirectUri: "https://app.example.com/silent-callback.html",
};

beforeEach(async () => {
  vi.resetModules();
  instances.length = 0;
  auth = await import("../auth");
});

describe("configureAuth", () => {
  it("returns false and configures nothing without authority/clientId", () => {
    expect(auth.configureAuth({ ...baseConfig, authority: "" })).toBe(false);
    expect(instances.length).toBe(0);
  });

  it("returns true and marks auth configured with a valid config", () => {
    expect(auth.configureAuth({ ...baseConfig })).toBe(true);
    expect(auth.isAuthConfigured()).toBe(true);
  });
});

describe("configureMediaAuth", () => {
  it("derives the conventional embedded defaults from the base path", () => {
    expect(auth.configureMediaAuth({ basePath: "/" })).toBe(true);
    expect(auth.isAuthConfigured()).toBe(true);
    // The delegation to resolveEmbeddedConfig keeps the picker on the same defaults as the gallery.
    expect(instances.length).toBe(1);
  });
});

describe("ensureAuthenticated", () => {
  it("returns 'authenticated' when a valid (non-expired) user already exists, without redirecting", async () => {
    auth.configureAuth({ ...baseConfig });
    lastInstance().getUser.mockResolvedValue({ access_token: "tok", expired: false });

    expect(await auth.ensureAuthenticated()).toBe("authenticated");
    expect(lastInstance().signinRedirect).not.toHaveBeenCalled();
  });

  it("acquires a token via silent sign-in when there is no current user", async () => {
    auth.configureAuth({ ...baseConfig, authFlow: "interactive" });
    lastInstance().getUser.mockResolvedValue(null);
    lastInstance().signinSilent.mockResolvedValue({ access_token: "tok2", expired: false });

    expect(await auth.ensureAuthenticated()).toBe("authenticated");
    expect(lastInstance().signinRedirect).not.toHaveBeenCalled();
  });

  it("returns 'failed' and does NOT redirect in silent (embedded) mode when no session can be established", async () => {
    auth.configureAuth({ ...baseConfig, authFlow: "silent" });
    lastInstance().getUser.mockResolvedValue(null);
    lastInstance().signinSilent.mockResolvedValue(null);

    expect(await auth.ensureAuthenticated()).toBe("failed");
    expect(lastInstance().signinRedirect).not.toHaveBeenCalled();
  });

  it("returns 'redirecting' and starts the interactive login exactly once when silent fails in interactive mode", async () => {
    auth.configureAuth({ ...baseConfig, authFlow: "interactive" });
    lastInstance().getUser.mockResolvedValue(null);
    lastInstance().signinSilent.mockRejectedValue(new Error("login_required"));

    expect(await auth.ensureAuthenticated()).toBe("redirecting");
    // getAccessToken's failure path and ensureAuthenticated's interactive branch share the guard,
    // so the underlying redirect is started only once.
    expect(lastInstance().signinRedirect).toHaveBeenCalledOnce();
  });

  it("returns 'failed' when the interactive redirect itself cannot start", async () => {
    auth.configureAuth({ ...baseConfig, authFlow: "interactive" });
    lastInstance().getUser.mockResolvedValue(null);
    lastInstance().signinSilent.mockResolvedValue(null);
    lastInstance().signinRedirect.mockRejectedValue(new Error("discovery unreachable"));

    expect(await auth.ensureAuthenticated()).toBe("failed");
    expect(lastInstance().signinRedirect).toHaveBeenCalled();
  });
});

describe("token renewal after session expiry", () => {
  it("starts a fresh interactive login when renewal fails in interactive mode", async () => {
    auth.configureAuth({ ...baseConfig, authFlow: "interactive" });
    lastInstance().signinSilent.mockRejectedValue(new Error("login_required"));

    expect(await auth.renewAccessToken()).toBeNull();
    expect(lastInstance().signinRedirect).toHaveBeenCalledOnce();
  });

  it("does NOT redirect on renewal failure in silent (embedded) mode", async () => {
    auth.configureAuth({ ...baseConfig, authFlow: "silent" });
    lastInstance().signinSilent.mockRejectedValue(new Error("login_required"));

    expect(await auth.renewAccessToken()).toBeNull();
    expect(lastInstance().signinRedirect).not.toHaveBeenCalled();
  });
});

describe("handleRedirectCallback", () => {
  it("returns false when no persisted settings exist (e.g. the page was opened directly)", async () => {
    window.sessionStorage.removeItem("media-gallery:oidc-settings");
    expect(await auth.handleRedirectCallback()).toBe(false);
  });

  it("returns true when the code exchange completes", async () => {
    auth.configureAuth({ ...baseConfig });
    expect(await auth.handleRedirectCallback()).toBe(true);
  });

  it("returns false when the code exchange throws, so the callback page can show the error", async () => {
    auth.configureAuth({ ...baseConfig });
    // The callback rebuilds its own UserManager; make the next instance's exchange fail.
    const { UserManager } = await import("oidc-client-ts");
    (UserManager as unknown as ReturnType<typeof vi.fn>).mockImplementationOnce(() => ({
      signinRedirectCallback: vi.fn().mockRejectedValue(new Error("access_denied")),
      events: { addUserLoaded: vi.fn(), addUserUnloaded: vi.fn() },
    }));

    expect(await auth.handleRedirectCallback()).toBe(false);
  });
});
