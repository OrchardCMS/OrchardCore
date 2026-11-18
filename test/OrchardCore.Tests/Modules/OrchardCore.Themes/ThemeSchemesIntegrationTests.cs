using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Themes;

/// <summary>
/// TheAdmin supports schemes: its pages carry the scheme and its stylesheet, Design > Themes links to its schemes,
/// and the schemes pages render.
/// </summary>
public sealed class ThemeSchemesIntegrationTests
{
    [Fact]
    public async Task TheAdmin_OffersItsSchemes_OnDesignThemes()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var themes = await GetPageAsync(context, "Admin/Themes");

        // The admin page is rendered in the site's default scheme of TheAdmin, with the theme's schemes stylesheet.
        Assert.Contains("data-ta-scheme=\"blue\"", themes, StringComparison.Ordinal);
        Assert.Contains("/Themes/schemes/TheAdmin.css?v=", themes, StringComparison.Ordinal);

        // Only TheAdmin supports schemes, so it alone has the button.
        Assert.Contains("/Admin/Themes/Schemes/TheAdmin\"", themes, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(themes, "/Admin/Themes/Schemes/"));

        var index = await GetPageAsync(context, "Admin/Themes/Schemes/TheAdmin");

        Assert.Contains("The Admin Theme schemes", index, StringComparison.Ordinal);
        Assert.Contains("/Admin/Themes/Schemes/TheAdmin/Edit/gold\"", index, StringComparison.Ordinal);
        Assert.Contains("/Admin/Themes/Schemes/TheAdmin/SetDefault/gray\"", index, StringComparison.Ordinal);
        Assert.DoesNotContain("/Admin/Themes/Schemes/TheAdmin/SetDefault/blue\"", index, StringComparison.Ordinal);
        Assert.DoesNotContain("/Admin/Themes/Schemes/TheAdmin/Delete/blue\"", index, StringComparison.Ordinal);

        var edit = await GetPageAsync(context, "Admin/Themes/Schemes/TheAdmin/Edit/gray");

        // TheAdmin's own preview, through the ThemeSchemePreview-TheAdmin alternate.
        Assert.Contains("ta-scheme-preview", edit, StringComparison.Ordinal);
        Assert.Contains("data-ta-scheme=\"editor-preview\"", edit, StringComparison.Ordinal);
        Assert.Contains("action-bar-bottom", edit, StringComparison.Ordinal);

        await GetPageAsync(context, "Admin/Themes/Schemes/TheAdmin/Create");

        var css = await GetPageAsync(context, "Themes/schemes/TheAdmin.css");

        Assert.Contains("[data-ta-scheme=\"blue\"]{", css, StringComparison.Ordinal);

        using var unknownList = await context.Client.GetAsync("Admin/Themes/Schemes/TheTheme", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, unknownList.StatusCode);

        using var unknownCss = await context.Client.GetAsync("Themes/schemes/TheTheme.css", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, unknownCss.StatusCode);
    }

    private static async Task<string> GetPageAsync(SiteContext context, string path)
    {
        using var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;

        for (var index = text.IndexOf(value, StringComparison.Ordinal); index >= 0; index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }
}
