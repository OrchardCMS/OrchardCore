using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.DisplayManagement;

public class ShapeHtmlRendererTests
{
    [Fact]
    public async Task RenderZoneAsync_ZoneWithShapes_RendersTheZoneOnly()
    {
        var zone = new Shape();
        var holder = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        holder.Zones["Content"] = zone;

        var displayHelper = new Mock<IDisplayHelper>();
        displayHelper
            .Setup(helper => helper.ShapeExecuteAsync(It.Is<IShape>(shape => ReferenceEquals(shape, zone))))
            .ReturnsAsync(new HtmlString("<p>Fields &amp; values</p>"));

        var html = await CreateRenderer(displayHelper.Object).RenderZoneAsync(holder, "Content");

        Assert.Equal("<p>Fields &amp; values</p>", html);
        displayHelper.Verify(helper => helper.ShapeExecuteAsync(It.Is<IShape>(shape => ReferenceEquals(shape, holder))), Times.Never());
    }

    [Fact]
    public async Task RenderZoneAsync_EmptyZoneOrNoZones_ReturnsEmpty()
    {
        var displayHelper = new Mock<IDisplayHelper>(MockBehavior.Strict);
        var renderer = CreateRenderer(displayHelper.Object);

        Assert.Empty(await renderer.RenderZoneAsync(new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape())), "Content"));
        Assert.Empty(await renderer.RenderZoneAsync(new Shape(), "Content"));
    }

    [Fact]
    public async Task RenderAsync_Shape_KeepsTheCurrentViewContext()
    {
        var shape = new Shape();
        var viewContextAccessor = new ViewContextAccessor();
        var displayHelper = new Mock<IDisplayHelper>();
        displayHelper.Setup(helper => helper.ShapeExecuteAsync(shape)).ReturnsAsync(new HtmlString("<b>Shape</b>"));

        var html = await CreateRenderer(displayHelper.Object, viewContextAccessor).RenderAsync(shape);

        Assert.Equal("<b>Shape</b>", html);
        Assert.Null(viewContextAccessor.ViewContext);
    }

    private static ShapeHtmlRenderer CreateRenderer(IDisplayHelper displayHelper, ViewContextAccessor viewContextAccessor = null)
    {
        return new ShapeHtmlRenderer(
            displayHelper,
            viewContextAccessor ?? new ViewContextAccessor(),
            new HttpContextAccessor(),
            Mock.Of<ITempDataProvider>(),
            HtmlEncoder.Default);
    }
}
