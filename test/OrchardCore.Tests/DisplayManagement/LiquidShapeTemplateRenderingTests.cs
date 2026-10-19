using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Localization;

namespace OrchardCore.Tests.DisplayManagement;

/// <summary>
/// Renders the pages of a tenant whose theme, 'TheBlogTheme', renders its shapes with Liquid templates.
/// </summary>
public class LiquidShapeTemplateRenderingTests
{
    // The localization context of a Liquid shape template is derived from its path.
    private const string BlogPostSummaryContext = "TheBlogTheme.Views.Content-BlogPost.Summary";

    [Fact]
    public async Task BlogHomePage_LiquidShapeTemplates_RenderNestedShapesAndLocalizeWithTheirTemplatePath()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();
        await TenantLocalizationTestHelper.EnableLocalizationAsync(context, ["en", "fr"]);
        await TenantLocalizationTestHelper.WritePoFileAsync(context, "fr", (BlogPostSummaryContext, "Posted by", "Publié par"));

        // Act. The home page of the tenant, relative to its URL prefix.
        var english = await TenantLocalizationTestHelper.GetPageAsync(context, string.Empty, "en");
        var french = await TenantLocalizationTestHelper.GetPageAsync(context, string.Empty, "fr");

        // Assert. The summary of the blog post (Content-BlogPost.Summary.liquid) is rendered in the list of
        // the blog (Blog-ListPart.liquid) displayed in the layout (Layout.liquid), its link by the 'a' tag.
        var title = english.QuerySelector(".post-preview a .post-title");
        Assert.NotNull(title);
        Assert.False(string.IsNullOrWhiteSpace(title.TextContent));
        Assert.Contains("Posted by", english.QuerySelector(".post-preview .post-meta").TextContent);
        Assert.Contains("Publié par", french.QuerySelector(".post-preview .post-meta").TextContent);
    }
}
