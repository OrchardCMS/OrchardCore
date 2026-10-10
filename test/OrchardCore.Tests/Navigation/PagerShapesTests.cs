using Microsoft.AspNetCore.Mvc.Rendering;
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
    public void Pager_PageSizeSelector_PageSizesArgument_IsWhatATemplateReadsFromModel()
    {
        // A theme template reads the page sizes through the dynamic Model. The argument must not be named "Items":
        // Shape already has an Items property (its child shapes), and dynamic binding prefers it over the argument.
        List<SelectListItem> pageSizes = [new SelectListItem { Text = "10", Value = "/admin?pageSize=10" }];

        var shape = new Shape();
        shape.Properties["PageSizes"] = pageSizes;
        shape.Properties["Items"] = pageSizes;

        dynamic model = shape;

        Assert.Same(pageSizes, model.PageSizes as List<SelectListItem>);
        Assert.Null(model.Items as List<SelectListItem>);
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
