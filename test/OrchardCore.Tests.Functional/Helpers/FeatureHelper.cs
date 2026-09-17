using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class FeatureHelper
{
    public static async Task EnableFeatureAsync(this IPage page, string prefix, string featureName)
    {
        await page.GotoAsync($"{prefix}/Admin/Features");
        await page.Locator($"#btn-enable-{ToFeatureButtonId(featureName)}").ClickAsync();
    }

    public static async Task DisableFeatureAsync(this IPage page, string prefix, string featureName)
    {
        await page.GotoAsync($"{prefix}/Admin/Features");
        await page.Locator($"#btn-disable-{ToFeatureButtonId(featureName)}").ClickAsync();
    }

    private static string ToFeatureButtonId(string featureName)
        => featureName.Replace('.', '_');
}
