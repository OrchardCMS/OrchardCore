using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Navigation;

namespace OrchardCore.Tests.Navigation;

public class PagerShapesTests
{
    [Fact]
    public void ActionLink_Disabled_RendersWithoutHrefAndRel()
    {
        var shape = new Shape();
        shape.Attributes["rel"] = "prev";

        var shapes = CreatePagerShapes();
        var html = ToHtmlString(shapes.ActionLink(shape, Mock.Of<IUrlHelper>(), "Previous", Disabled: true));

        Assert.Equal("<a>Previous</a>", html);
    }

    [Fact]
    public void ActionLink_Enabled_KeepsRel()
    {
        var shape = new Shape();
        shape.Attributes["rel"] = "next";

        var url = new Mock<IUrlHelper>();
        url.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/blog?pagenum=2");

        var shapes = CreatePagerShapes();
        var html = ToHtmlString(shapes.ActionLink(shape, url.Object, "Next"));

        Assert.Contains("href=\"/blog?pagenum=2\"", html);
        Assert.Contains("rel=\"next\"", html);
    }

    private static PagerShapes CreatePagerShapes()
    {
        var localizer = new Mock<IStringLocalizer<PagerShapes>>();
        localizer.Setup(l => l[It.IsAny<string>()])
            .Returns<string>(name => new LocalizedString(name, name));

        return new PagerShapes(localizer.Object);
    }

    private static string ToHtmlString(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}
