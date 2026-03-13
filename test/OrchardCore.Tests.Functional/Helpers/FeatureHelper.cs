using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class FeatureHelper
{
    public static async Task EnableFeatureAsync(IPage page, string prefix, string featureName)
    {
        await page.GotoAsync($"{prefix}/Admin/Features");
        await page.Locator($"#btn-enable-{featureName}").ClickAsync();
    }

    public static async Task DisableFeatureAsync(IPage page, string prefix, string featureName)
    {
        await page.GotoAsync($"{prefix}/Admin/Features");
        await page.Locator($"#btn-disable-{featureName}").ClickAsync();
    }
}
