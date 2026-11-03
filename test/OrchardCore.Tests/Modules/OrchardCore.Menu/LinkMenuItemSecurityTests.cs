using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Menu;
using OrchardCore.Menu.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Menu;

public class LinkMenuItemSecurityTests
{
    [Theory]
    [InlineData("javascript:alert(1)", "")]
    [InlineData("data:text/html,<script>alert(1)</script>", "")]
    [InlineData("http://[", "")]
    [InlineData("https://example.com/path", "https://example.com/path")]
    [InlineData("/relative/path?value=1#fragment", "/relative/path?value=1#fragment")]
    [InlineData("~/relative/path", "~/relative/path")]
    [InlineData("#section", "#section")]
    [InlineData("", "")]
    public async Task MenuItemLink_Displaying_ProtectsUrlWithoutChangingSource(string url, string expectedUrl)
    {
        var contentItem = new ContentItem
        {
            ContentType = "LinkMenuItem",
            DisplayText = "Link title",
        };
        contentItem.Weld(new LinkMenuItemPart { Url = url, Target = "_blank" });
        var shape = new Shape();
        shape.Properties["ContentItem"] = contentItem;
        shape.Properties["Level"] = 0;

        using var services = new ServiceCollection()
            .AddOptions()
            .AddScoped<IHtmlSanitizerService, HtmlSanitizerService>()
            .BuildServiceProvider();
        var builder = new ShapeTableBuilder(Mock.Of<IFeatureInfo>());
        await new MenuShapes().DiscoverAsync(builder);
        var descriptor = new ShapeDescriptor { ShapeType = "MenuItemLink" };
        foreach (var alteration in builder.BuildAlterations().Where(a => a.ShapeType == "MenuItemLink"))
        {
            alteration.Alter(descriptor);
        }

        var context = new ShapeDisplayContext { Shape = shape, ServiceProvider = services };
        foreach (var displaying in descriptor.DisplayingAsync)
        {
            await displaying(context);
        }

        var renderedItem = Assert.IsType<ContentItem>(shape.Properties["ContentItem"]);
        Assert.True(renderedItem.TryGet<LinkMenuItemPart>(out var renderedPart));
        Assert.Equal(expectedUrl, renderedPart.Url);
        Assert.Equal("_blank", renderedPart.Target);
        Assert.Equal("Link title", renderedItem.DisplayText);
        Assert.True(contentItem.TryGet<LinkMenuItemPart>(out var originalPart));
        Assert.Equal(url, originalPart.Url);
        if (url == expectedUrl)
        {
            Assert.Same(contentItem, renderedItem);
        }
        else
        {
            Assert.NotSame(contentItem, renderedItem);
        }
    }
}
