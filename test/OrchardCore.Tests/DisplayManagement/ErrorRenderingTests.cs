using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.DisplayManagement;

public class ErrorRenderingTests
{
    [Fact]
    public async Task AddAsync_PositionWrapper_ThrowsNotSupportedException()
    {
        var wrapper = (IShape)PositionWrapper.TryWrap(new HtmlString("Rendered zone"), "");

        await Assert.ThrowsAsync<NotSupportedException>(async () => await wrapper.AddAsync(new Shape(), ""));
    }

    [Theory]
    [InlineData("Content")]
    [InlineData("LaterZone")]
    [InlineData("Layout")]
    [InlineData("AfterLayout")]
    public async Task ExecuteAsync_RenderingFails_ResetsLayoutForRetry(string failurePoint)
    {
        var displayHelper = new TestDisplayHelper
        {
            FailLayout = failurePoint == "Layout",
            FailAfterLayout = failurePoint == "AfterLayout",
        };
        await using var services = new ServiceCollection()
            .AddSingleton<IDisplayHelper>(displayHelper)
            .AddScoped<ViewContextAccessor>()
            .BuildServiceProvider();
        await using var scope = services.CreateAsyncScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        var layout = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        layout.Metadata.Type = "Layout";
        context.Features.Set(new RazorViewFeature { ThemeLayout = layout });
        await layout.Zones["Footer"].AddAsync(new HtmlString("widget"));
        if (failurePoint is "Content" or "LaterZone")
        {
            await layout.Zones[failurePoint].AddAsync(new Shape { Metadata = { Type = "Fail" } });
        }

        using var writer = new StringWriter();
        var page = new ThemeLayout
        {
            ViewContext = new ViewContext
            {
                HttpContext = context,
                View = Mock.Of<IView>(),
                Writer = writer,
            },
            BodyContent = new HtmlString("Original body"),
            HtmlEncoder = HtmlEncoder.Default,
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(page.ExecuteAsync);

        Assert.Same(displayHelper.RenderingFailure, exception);
        Assert.Same(layout, page.ThemeLayout);
        Assert.Same(layout, context.Features.Get<RazorViewFeature>().ThemeLayout);
        Assert.Null(layout.Metadata.ChildContent);
        Assert.False(layout.Zones.IsNotEmpty("Content"));
        Assert.False(layout.Zones.IsNotEmpty("Footer"));
        Assert.False(layout.Zones.IsNotEmpty("LaterZone"));
        Assert.Equal(string.Empty, writer.ToString());

        await layout.Zones["Footer"].AddAsync(new HtmlString("widget"));
        page.BodyContent = new HtmlString("Error body");
        await page.ExecuteAsync();

        var response = writer.ToString();
        Assert.Contains("Error body", response);
        Assert.DoesNotContain("Original body", response);
        Assert.Equal(2, response.Split("widget", StringSplitOptions.None).Length);
        Assert.NotNull(layout.Metadata.ChildContent);
        Assert.True(layout.Zones.IsNotEmpty("Content"));
        Assert.True(layout.Zones.IsNotEmpty("Footer"));
    }

    private sealed class TestDisplayHelper : IDisplayHelper
    {
        public InvalidOperationException RenderingFailure { get; } = new("Layout rendering failed.");

        public bool FailLayout { get; set; }
        public bool FailAfterLayout { get; set; }

        public async Task<IHtmlContent> ShapeExecuteAsync(IShape shape)
        {
            if (shape.Metadata.Type == "Fail")
            {
                throw RenderingFailure;
            }

            if (shape is IHtmlContent rendered)
            {
                return rendered;
            }

            if (shape is IZoneHolding && FailLayout)
            {
                FailLayout = false;
                throw RenderingFailure;
            }

            if (shape.Metadata.ChildContent is not null)
            {
                return shape.Metadata.ChildContent;
            }

            var content = new HtmlContentBuilder();
            if (shape is IZoneHolding)
            {
                foreach (var zone in shape.Properties.Values.OfType<IHtmlContent>())
                {
                    content.AppendHtml(zone);
                }
            }
            else
            {
                foreach (var item in shape.Items)
                {
                    if (item is IHtmlContent html)
                    {
                        content.AppendHtml(html);
                    }
                    else if (item is IShape child)
                    {
                        content.AppendHtml(await ShapeExecuteAsync(child));
                    }
                }
            }

            shape.Metadata.ChildContent = content;

            if (shape is IZoneHolding && FailAfterLayout)
            {
                FailAfterLayout = false;
                throw RenderingFailure;
            }

            return content;
        }
    }
}
