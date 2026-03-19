using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public static class FeatureHelper
{
    public static async Task EnableFeatureAsync(IPage page, string prefix, string featureName)
    {
        await page.GotoAsync($"{prefix}/Admin/Features");
        await page.Locator($"#btn-enable-{SanitizeId(featureName)}").ClickAsync();
    }

    public static async Task DisableFeatureAsync(IPage page, string prefix, string featureName)
    {
        await page.GotoAsync($"{prefix}/Admin/Features");
        await page.Locator($"#btn-disable-{SanitizeId(featureName)}").ClickAsync();
    }

    /// <summary>
    /// Mirrors <c>Html.GenerateIdFromName()</c> which replaces dots and other
    /// non-identifier characters with underscores when generating HTML element IDs.
    /// </summary>
    private static string SanitizeId(string name) => name.Replace('.', '_');
}
