using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Liquid;
using OrchardCore.Modules;

namespace OrchardCore.Tests.DisplayManagement;

public class LiquidShapeTemplateViewEngineTests
{
    [Fact]
    public async Task RenderAsync_CurrentView_RendersTemplateWithItsOwnViewContext()
    {
        // Arrange
        var folder = Directory.CreateTempSubdirectory("liquid-engine-");

        try
        {
            Directory.CreateDirectory(Path.Combine(folder.FullName, "Views"));
            await File.WriteAllTextAsync(Path.Combine(folder.FullName, "Views", "Item.liquid"), "<p>{{ 'Item' | view_context }}</p>", TestContext.Current.CancellationToken);

            var shape = new Shape();
            shape.Metadata.Type = "Item";

            ViewContext templateViewContext = null;
            ViewContextAccessor viewContextAccessor = null;
            var currentWhileRendering = false;

            var services = new ServiceCollection()
                .AddSingleton<LiquidViewParser>()
                .AddSingleton<IHostEnvironment>(Mock.Of<IHostEnvironment>(environment => environment.EnvironmentName == Environments.Production))
                .AddSingleton(HtmlEncoder.Default)
                .AddSingleton(CreateLocalClock())
                .AddScoped<ViewContextAccessor>()
                .AddScoped<LiquidShapeTemplateViewEngine>()
                .AddScoped(sp => (RazorShapeTemplateViewEngine)null)
                .Configure<TemplateOptions>(options =>
                {
                    options.FileProvider = new PhysicalFileProvider(folder.FullName);
                    options.Filters.AddFilter("view_context", (input, arguments, context) =>
                    {
                        templateViewContext = ((LiquidTemplateContext)context).ViewContext;
                        currentWhileRendering = viewContextAccessor.ViewContext == templateViewContext;

                        return ValueTask.FromResult<FluidValue>(input);
                    });
                })
                .BuildServiceProvider();

            var shellContext = new ShellContext
            {
                Settings = new ShellSettings().AsDefaultShell().AsRunning(),
                ServiceProvider = services,
            };

            IHtmlContent result = null;
            ViewContext outerViewContext = null;

            await (await shellContext.CreateScopeAsync()).UsingAsync(async scope =>
            {
                viewContextAccessor = scope.ServiceProvider.GetRequiredService<ViewContextAccessor>();
                var httpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
                outerViewContext = new ViewContext(
                    new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
                    Mock.Of<IView>(),
                    new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = "Outer" },
                    new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
                    TextWriter.Null,
                    new HtmlHelperOptions());
                viewContextAccessor.ViewContext = outerViewContext;

                var engine = scope.ServiceProvider.GetRequiredService<LiquidShapeTemplateViewEngine>();

                // Act
                result = await engine.RenderAsync("Views/Item.liquid", new DisplayContext
                {
                    Value = shape,
                    HtmlFieldPrefix = "Item",
                    ServiceProvider = scope.ServiceProvider,
                });

                // Assert
                using var writer = new StringWriter();
                result.WriteTo(writer, HtmlEncoder.Default);
                Assert.Equal("<p>Item</p>", writer.ToString());
                Assert.Same(outerViewContext, viewContextAccessor.ViewContext);
            }, activateShell: false);

            // As when rendered by its Razor page as a partial view, the template has its own view context, which is
            // the current one while it is rendered, whose model is the shape, and whose file path is the Razor page.
            Assert.NotNull(templateViewContext);
            Assert.NotSame(outerViewContext, templateViewContext);
            Assert.True(currentWhileRendering);
            Assert.Same(shape, templateViewContext.ViewData.Model);
            Assert.Equal("Item", templateViewContext.ViewData.TemplateInfo.HtmlFieldPrefix);
            Assert.Equal("/Views/Item.cshtml", templateViewContext.ExecutingFilePath);
            Assert.Equal("Outer", outerViewContext.ViewData.Model);
            Assert.Equal(string.Empty, outerViewContext.ViewData.TemplateInfo.HtmlFieldPrefix);
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    private static ILocalClock CreateLocalClock()
    {
        var localClock = new Mock<ILocalClock>();
        localClock.Setup(clock => clock.GetLocalNowAsync()).ReturnsAsync(DateTimeOffset.UtcNow);
        localClock.Setup(clock => clock.GetLocalTimeZoneAsync())
            .ReturnsAsync(Mock.Of<ITimeZone>(timeZone => timeZone.TimeZoneId == "UTC"));

        return localClock.Object;
    }
}
