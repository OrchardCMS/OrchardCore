using OrchardCore.ContentManagement;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Menu;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Menu;

public class MenuShapesTests
{
    private static readonly IHtmlSanitizerService _sanitizer =
        new HtmlSanitizerService(Options.Create(new HtmlSanitizerOptions()));

    [Theory]
    [InlineData("https://orchardcore.net/", true)]
    [InlineData("/docs/reference", true)]
    [InlineData("#section", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("JaVaScRiPt:alert(1)", false)]
    [InlineData("data:text/html,<script>alert(1)</script>", false)]
    [InlineData("http://[", false)]
    public void IsSafeUrlShouldRejectUnsafeSchemes(string url, bool expected)
    {
        Assert.Equal(expected, MenuShapes.IsSafeUrl(url, _sanitizer, HtmlEncoder.Default));
    }

    [Fact]
    public void CreateSafeContentItemShouldNotMutatePersistedContent()
    {
        const string html = "<p>Safe</p><script>alert(1)</script>";
        const string url = "javascript:alert(1)";
        var contentItem = new ContentItem
        {
            ContentType = "HtmlMenuItem",
        };
        contentItem.Weld(new HtmlMenuItemPart
        {
            Html = html,
            Url = url,
        });

        var renderedContentItem = MenuShapes.CreateSafeContentItem(
            contentItem,
            new HtmlMenuItemPartSettings { SanitizeHtml = true },
            _sanitizer,
            HtmlEncoder.Default);

        var sourcePart = contentItem.As<HtmlMenuItemPart>();
        var renderedPart = renderedContentItem.As<HtmlMenuItemPart>();

        Assert.Equal(html, sourcePart.Html);
        Assert.Equal(url, sourcePart.Url);
        Assert.Equal("<p>Safe</p>", renderedPart.Html);
        Assert.Empty(renderedPart.Url);
    }

    [Fact]
    public void CreateSafeContentItemShouldPreserveTrustedHtmlWhenSanitizationIsDisabled()
    {
        const string html = "<p>Safe</p><script>alert(1)</script>";
        var contentItem = new ContentItem
        {
            ContentType = "HtmlMenuItem",
        };
        contentItem.Weld(new HtmlMenuItemPart
        {
            Html = html,
            Url = "/safe",
        });

        var renderedContentItem = MenuShapes.CreateSafeContentItem(
            contentItem,
            new HtmlMenuItemPartSettings { SanitizeHtml = false },
            _sanitizer,
            HtmlEncoder.Default);

        var renderedPart = renderedContentItem.As<HtmlMenuItemPart>();

        Assert.Equal(html, renderedPart.Html);
        Assert.Equal("/safe", renderedPart.Url);
    }
}
