using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Hosting;
using OrchardCore.Diagnostics;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.DisplayManagement;

public class ErrorRenderingTests
{
    [Theory]
    [InlineData("Content")]
    [InlineData("LaterZone")]
    [InlineData("Layout")]
    [InlineData("BeforeRendering")]
    [InlineData("StatusCode")]
    [InlineData("StatusCodeThenException")]
    public async Task ReExecute_FailedOrStatusCodeResponse_UsesFreshLayoutAndTitle(string failurePoint)
    {
        await using var services = new ServiceCollection()
            .AddLogging()
            .AddOptions()
            .AddMetrics()
            .AddSingleton(new DiagnosticListener(nameof(ErrorRenderingTests)))
            .AddSingleton<IShapeFactory>(new TestShapeFactory())
            .AddSingleton<IDisplayHelper>(new TestDisplayHelper())
            .AddScoped<ILayoutAccessor, LayoutAccessor>()
            .AddScoped<IPageTitleBuilder, PageTitleBuilder>()
            .AddScoped<ViewContextAccessor>()
            .BuildServiceProvider();
        await using var originalScope = services.CreateAsyncScope();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = originalScope.ServiceProvider,
        };
        httpContext.Request.Path = "/original";
        using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;

        IZoneHolding originalLayout = null;
        IPageTitleBuilder originalTitle = null;
        var errorExecutions = 0;
        var renderingFailures = new List<(Exception Exception, IZoneHolding FeatureLayout, IZoneHolding PageLayout)>();
        var builder = new ApplicationBuilder(services);
        var environment = Mock.Of<IHostEnvironment>(host => host.EnvironmentName == Environments.Production);

        new DiagnosticsStartupFilter(environment).Configure(app => app.Run(async context =>
        {
            await new RazorViewActionFilter().OnActionExecutionAsync(new ActionContext { HttpContext = context });
            var layout = await context.RequestServices.GetRequiredService<ILayoutAccessor>().GetLayoutAsync();
            var title = context.RequestServices.GetRequiredService<IPageTitleBuilder>();
            Assert.Same(layout, context.Features.Get<RazorViewFeature>().ThemeLayout);

            // Model a widget added by a result filter on every execution.
            await layout.Zones["Footer"].AddAsync(new HtmlString("widget"));

            if (context.Request.Path == "/original")
            {
                originalLayout = layout;
                originalTitle = title;
                title.AddSegment(new HtmlString("Original title"), "0");

                if (failurePoint is "StatusCode" or "StatusCodeThenException")
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                if (failurePoint == "BeforeRendering")
                {
                    throw new InvalidOperationException("Failure before layout rendering.");
                }

                if (failurePoint == "Layout")
                {
                    ((IShape)layout).Metadata.Type = "Fail";
                }
                else
                {
                    await layout.Zones[failurePoint].AddAsync(new Shape { Metadata = new ShapeMetadata { Type = "Fail" } });
                }

                await RenderAsync(context, "Original body", renderingFailures);
                return;
            }

            errorExecutions++;
            Assert.NotSame(originalLayout, layout);
            Assert.NotSame(originalTitle, title);
            Assert.Equal(string.Empty, ToHtml(title.GenerateTitle()));

            // Repeated filter calls within one execution must retain its layout and title.
            title.AddSegment(new HtmlString("Error title"), "0");
            await new RazorViewActionFilter().OnActionExecutionAsync(new ActionContext { HttpContext = context });
            Assert.Same(layout, context.Features.Get<RazorViewFeature>().ThemeLayout);
            Assert.Equal("Error title", ToHtml(title.GenerateTitle()));

            if (failurePoint == "StatusCodeThenException" && context.Request.Path == "/Error/404")
            {
                await layout.Zones["LaterZone"].AddAsync(new Shape { Metadata = new ShapeMetadata { Type = "Fail" } });
                await RenderAsync(context, "404 body", renderingFailures);
                return;
            }

            var html = await RenderAsync(context, "Error body");
            await context.Response.WriteAsync(ToHtml(title.GenerateTitle()) + html);
        }))(builder);

        await builder.Build()(httpContext);

        Assert.Equal(failurePoint == "StatusCode" ? 404 : 500, httpContext.Response.StatusCode);
        Assert.Equal(failurePoint == "StatusCodeThenException" ? 2 : 1, errorExecutions);
        if (failurePoint is "BeforeRendering" or "StatusCode")
        {
            Assert.Empty(renderingFailures);
        }
        else
        {
            var failure = Assert.Single(renderingFailures);
            Assert.Same(((TestDisplayHelper)services.GetRequiredService<IDisplayHelper>()).RenderingFailure, failure.Exception);
            Assert.Null(failure.FeatureLayout);
            Assert.Null(failure.PageLayout);
        }

        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody);
        var response = await reader.ReadToEndAsync();
        Assert.Contains("Error title", response);
        Assert.Contains("Error body", response);
        Assert.DoesNotContain("Original", response);
        Assert.Equal(2, response.Split("widget", StringSplitOptions.None).Length);
    }

    [Fact]
    public async Task AddAsync_PositionWrapper_ThrowsNotSupportedException()
    {
        var wrapper = (IShape)PositionWrapper.TryWrap(new HtmlString("Rendered zone"), "");

        await Assert.ThrowsAsync<NotSupportedException>(async () => await wrapper.AddAsync(new Shape(), ""));
    }

    private static async Task<string> RenderAsync(
        HttpContext context,
        string body,
        List<(Exception Exception, IZoneHolding FeatureLayout, IZoneHolding PageLayout)> renderingFailures = null)
    {
        using var writer = new StringWriter();
        var page = new ThemeLayout
        {
            ViewContext = new ViewContext
            {
                HttpContext = context,
                View = Mock.Of<IView>(),
                Writer = writer,
            },
            BodyContent = new HtmlString(body),
            HtmlEncoder = HtmlEncoder.Default,
        };

        try
        {
            await page.ExecuteAsync();
        }
        catch (Exception exception)
        {
            renderingFailures?.Add((exception, context.Features.Get<RazorViewFeature>().ThemeLayout, page.ThemeLayout));
            throw;
        }

        return writer.ToString();
    }

    private static string ToHtml(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    private sealed class TestShapeFactory : IShapeFactory
    {
        public dynamic New => throw new NotSupportedException();

        public async ValueTask<IShape> CreateAsync(
            string shapeType,
            Func<ValueTask<IShape>> shapeFactory,
            Action<ShapeCreatingContext> creating,
            Action<ShapeCreatedContext> created)
        {
            var shape = shapeFactory is null ? new Shape() : await shapeFactory();
            shape.Metadata.Type = shapeType;
            return shape;
        }
    }

    private sealed class TestDisplayHelper : IDisplayHelper
    {
        public InvalidOperationException RenderingFailure { get; } = new("Layout rendering failed.");

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

            return content;
        }
    }
}
