using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class AuthHelper
{
    public static async Task LoginAsync(this IPage page, string prefix = "", OrchardConfig config = null)
    {
        config ??= TestUtils.DefaultConfig;
        await page.GotoAsync($"{prefix}/login");

        // If already logged in (redirected away from login), skip.
        if (!page.Url.Contains("/login", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await page.Locator("#LoginForm_UserName").FillAsync(config.Username);
        await page.Locator("#LoginForm_Password").FillAsync(config.Password);
        await page.Locator("button[type=\"submit\"]").ClickAsync();

        // Widen past Playwright's 30s default: observed live on CI (job 109531180853,
        // Redis + Azurite backend) timing out here under heavy runner contention, even
        // though this is just the login redirect + its page assets settling - the same
        // class of runner-contention margin other tests in this suite have needed (see
        // ShortcodeModalTests/PredefinedListEditorTests history). This helper is used by
        // nearly every test, so a login-page timeout here is a single point of failure
        // for the whole suite regardless of what any individual test is actually about.
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 60_000 });
    }
}
