using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Themes.TheAdmin;

/// <summary>
/// TheAdmin's schemes: its pages carry the scheme and its stylesheet, its card on Design > Themes links to its
/// schemes, and the schemes pages render.
/// </summary>
public sealed class AdminSchemesIntegrationTests
{
    [Fact]
    public async Task TheAdmin_OffersItsSchemes_OnDesignThemes()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var themes = await GetPageAsync(context, "Admin/Themes");

        // The admin page is rendered in the site's default scheme, with the schemes stylesheet, versioned by a short
        // hash of its text.
        Assert.Contains("data-ta-scheme=\"gray\"", themes, StringComparison.Ordinal);
        Assert.Matches(@"/TheAdmin/schemes\.css\?v=[0-9a-f]{16}""", themes);

        // TheAdmin's card alone has the button.
        Assert.Equal(1, CountOccurrences(themes, "/Admin/TheAdmin/Schemes\""));

        var index = await GetPageAsync(context, "Admin/TheAdmin/Schemes");

        Assert.Contains("Admin schemes", index, StringComparison.Ordinal);
        Assert.Contains("/Admin/TheAdmin/Schemes/Edit/gold\"", index, StringComparison.Ordinal);
        Assert.Contains("/Admin/TheAdmin/Schemes/SetDefault/blue\"", index, StringComparison.Ordinal);
        Assert.DoesNotContain("/Admin/TheAdmin/Schemes/SetDefault/gray\"", index, StringComparison.Ordinal);
        Assert.DoesNotContain("/Admin/TheAdmin/Schemes/Delete/gray\"", index, StringComparison.Ordinal);

        var edit = await GetPageAsync(context, "Admin/TheAdmin/Schemes/Edit/gray");

        Assert.Contains("ta-scheme-preview", edit, StringComparison.Ordinal);
        Assert.Contains("data-ta-scheme=\"editor-preview\"", edit, StringComparison.Ordinal);
        Assert.Contains("action-bar-bottom", edit, StringComparison.Ordinal);

        await GetPageAsync(context, "Admin/TheAdmin/Schemes/Create");

        var css = await GetPageAsync(context, "TheAdmin/schemes.css");

        Assert.Contains("[data-ta-scheme=\"blue\"]{--ta-primary:", css, StringComparison.Ordinal);
        Assert.Contains("--ta-navy:", css, StringComparison.Ordinal);
        Assert.DoesNotContain("--bs-", css, StringComparison.Ordinal);

        using var unknown = await context.Client.GetAsync("Admin/TheAdmin/Schemes/Edit/missing", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
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
