using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class AuthHelper
{
    public static async Task LoginAsync(IPage page, string prefix = "", OrchardConfig config = null)
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
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
