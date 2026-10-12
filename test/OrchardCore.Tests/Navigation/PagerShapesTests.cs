using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Navigation;

namespace OrchardCore.Tests.Navigation;

public class PagerShapesTests
{
    [Fact]
    public void Pager_PageSizeSelector_IsProvidedAsShape_WithoutThemeTemplate()
    {
        // Themes that don't provide a "Pager_PageSizeSelector" template rely on this default implementation.
        var method = typeof(PagerShapes).GetMethod(nameof(PagerShapes.Pager_PageSizeSelector));

        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttributes(typeof(ShapeAttribute), false).SingleOrDefault());
    }

    [Fact]
    public void Pager_PageSizeSelector_WithItems_RendersSelectWithSelectedOption()
    {
        var shapes = CreatePagerShapes();

        var result = shapes.Pager_PageSizeSelector(
        [
            new SelectListItem { Text = "10", Value = "/blog?pageSize=10" },
            new SelectListItem { Text = "25", Value = "/blog?q=a&pageSize=25", Selected = true },
        ]);

        var html = ToHtmlString(result);

        Assert.Contains("<div class=\"pager-page-size\"><label>Items per page</label><select", html);
        Assert.Contains("aria-label=\"Items per page\"", html);
        Assert.Contains("<option value=\"/blog?pageSize=10\">10</option>", html);
        Assert.Contains("<option selected=\"selected\" value=\"/blog?q=a&amp;pageSize=25\">25</option>", html);
    }

    [Fact]
    public void Pager_PageSizeSelector_WithoutItems_RendersNothing()
    {
        var shapes = CreatePagerShapes();

        Assert.Empty(ToHtmlString(shapes.Pager_PageSizeSelector(null)));
        Assert.Empty(ToHtmlString(shapes.Pager_PageSizeSelector([])));
    }

    [Fact]
    public void ActionLink_Disabled_RendersWithoutHrefAndRel()
    {
        var shape = new Shape();
        shape.Attributes["rel"] = "prev";

        var html = ToHtmlString(PagerShapes.ActionLink(shape, Mock.Of<IUrlHelper>(), "Previous", Disabled: true));

        Assert.Equal("<a>Previous</a>", html);
    }

    [Fact]
    public void ActionLink_Enabled_KeepsRel()
    {
        var shape = new Shape();
        shape.Attributes["rel"] = "next";

        var url = new Mock<IUrlHelper>();
        url.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/blog?pagenum=2");

        var html = ToHtmlString(PagerShapes.ActionLink(shape, url.Object, "Next"));

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
