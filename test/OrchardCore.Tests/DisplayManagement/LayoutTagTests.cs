using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Liquid;

namespace OrchardCore.Tests.DisplayManagement;

public class LayoutTagTests
{
    [Fact]
    public async Task WriteToAsync_ShapeTemplateWithoutRazorPage_AddsAlternateToThemeLayout()
    {
        // Arrange. Liquid shape templates are rendered without a Razor page.
        var layout = new ZoneHolding(() => ValueTask.FromResult<IShape>(null));
        layout.Metadata.Type = "Layout";

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set(new RazorViewFeature { ThemeLayout = layout });

        var services = new ServiceCollection()
            .AddSingleton(new ViewContextAccessor { ViewContext = new ViewContext { HttpContext = httpContext, View = Mock.Of<IView>() } })
            .BuildServiceProvider();

        var context = new LiquidTemplateContext(services, new TemplateOptions());

        // Act
        await LayoutTag.WriteToAsync(new LiteralExpression(new StringValue("Layout__Landing")), TextWriter.Null, NullEncoder.Default, context);
        await LayoutTag.WriteToAsync(new LiteralExpression(new StringValue("Layout__Landing")), TextWriter.Null, NullEncoder.Default, context);

        // Assert
        Assert.Equal(["Layout__Landing"], layout.Metadata.Alternates);
    }
}
