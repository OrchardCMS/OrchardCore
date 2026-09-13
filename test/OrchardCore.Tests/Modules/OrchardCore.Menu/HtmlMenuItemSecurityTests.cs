using OrchardCore.ContentManagement;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Menu;
using OrchardCore.Menu.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Menu;

public class HtmlMenuItemSecurityTests
{
    [Theory]
    [InlineData("/", true)]
    [InlineData("/relative/path?value=1#fragment", true)]
    [InlineData("https://example.com/path", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("data:text/html,<script>alert(1)</script>", false)]
    [InlineData("http://[", false)]
    public void IsSafeUrl_ValidatesSchemesWithoutThrowing(string url, bool expected)
    {
        var sanitizer = CreateSanitizer();

        Assert.Equal(expected, MenuShapes.IsSafeUrl(url, sanitizer));
    }

    [Fact]
    public void CreateSafeMenuItem_SanitizesCloneWithoutChangingPersistedSource()
    {
        var contentItem = new ContentItem();
        contentItem.Apply(
            nameof(HtmlMenuItemPart),
            new HtmlMenuItemPart
            {
                Html = "<script>alert('xss')</script><strong>Safe</strong>",
                Url = "javascript:alert(1)",
            });

        var safeContentItem = MenuShapes.CreateSafeMenuItem(contentItem, true, CreateSanitizer());

        Assert.True(contentItem.TryGet<HtmlMenuItemPart>(out var originalPart));
        Assert.Equal("<script>alert('xss')</script><strong>Safe</strong>", originalPart.Html);
        Assert.Equal("javascript:alert(1)", originalPart.Url);
        Assert.True(safeContentItem.TryGet<HtmlMenuItemPart>(out var safePart));
        Assert.Equal("<strong>Safe</strong>", safePart.Html);
        Assert.Equal(string.Empty, safePart.Url);
    }

    private static IHtmlSanitizerService CreateSanitizer()
    {
        var services = new ServiceCollection();
        services.AddOptions<HtmlSanitizerOptions>();
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

        return services.BuildServiceProvider().GetRequiredService<IHtmlSanitizerService>();
    }
}
