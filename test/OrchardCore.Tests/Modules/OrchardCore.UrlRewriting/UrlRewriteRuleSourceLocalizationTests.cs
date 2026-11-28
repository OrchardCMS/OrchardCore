using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Localization;

namespace OrchardCore.Tests.Modules.OrchardCore.UrlRewriting;

/// <summary>
/// Renders the page that adds a URL rewrite rule, and checks that the name of its source, declared as a static
/// <c>LocalizationSource</c> member, uses the PO file translations of the tenant in the culture of the request.
/// </summary>
public class UrlRewriteRuleSourceLocalizationTests
{
    [Fact]
    public async Task Sources_CultureOfTheRequest_UseTheTranslationsOfTheTenant()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();

        await TenantLocalizationTestHelper.WritePoFileAsync(
            context,
            "fr",
            ("OrchardCore.UrlRewriting.Services.UrlRedirectRuleSource", "Redirect", "Redirection"));

        await TenantLocalizationTestHelper.EnableLocalizationAsync(context, ["en", "fr"], "OrchardCore.UrlRewriting");

        // Act
        var french = await TenantLocalizationTestHelper.GetPageAsync(context, "Admin/UrlRewriting/Create/Redirect", "fr");
        var english = await TenantLocalizationTestHelper.GetPageAsync(context, "Admin/UrlRewriting/Create/Redirect", "en");
        var frenchAgain = await TenantLocalizationTestHelper.GetPageAsync(context, "Admin/UrlRewriting/Create/Redirect", "fr");

        // Assert. The rule sources are singletons, so a translation frozen by the first request fails.
        Assert.Equal("New 'Redirection' rule", Assert.Single(french.QuerySelectorAll("h1.oc-breadcrumb-title")).TextContent);
        Assert.Equal("New 'Redirect' rule", Assert.Single(english.QuerySelectorAll("h1.oc-breadcrumb-title")).TextContent);
        Assert.Equal("New 'Redirection' rule", Assert.Single(frenchAgain.QuerySelectorAll("h1.oc-breadcrumb-title")).TextContent);
    }
}
