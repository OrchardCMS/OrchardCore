using Fluid;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.Liquid;
using OrchardCore.Modules;

namespace OrchardCore.Tests.DisplayManagement;

public class LiquidViewTemplateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RenderAsync_ExistingViewContext_LocalizesAndRestoresScope(bool useWriter)
    {
        using var services = CreateServiceProvider();
        var options = new TemplateOptions();
        options.Filters.AddFilter("t", LiquidViewFilters.Localize);
        var context = new LiquidTemplateContext(services, options);
        context.SetValue("Model", "Outer");
        var parser = new FluidParser();
        Assert.True(parser.TryParse("{{ Model }}: {{ 'Name' | t }}", out var template, out var error), error);

        var result = await RenderAsync(template, context, useWriter);

        Assert.Equal("Inner: Localized name", result);
        Assert.Equal("Outer", context.GetValue("Model").ToStringValue());
        Assert.Null(context.GetValue("ViewLocalizer").ToObjectValue());
        Mock.Get(services.GetRequiredService<IViewLocalizer>())
            .As<IViewContextAware>()
            .Verify(localizer => localizer.Contextualize(context.ViewContext), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RenderAsync_RenderingThrows_RestoresScope(bool useWriter)
    {
        using var services = CreateServiceProvider();
        var options = new TemplateOptions();
        options.Filters.AddFilter("fail", (_, _, _) => throw new InvalidOperationException("Rendering failed."));
        var context = new LiquidTemplateContext(services, options);
        context.SetValue("Model", "Outer");
        var parser = new FluidParser();
        Assert.True(parser.TryParse("{{ Model | fail }}", out var template, out var error), error);

        await Assert.ThrowsAsync<InvalidOperationException>(() => RenderAsync(template, context, useWriter));

        Assert.Equal("Outer", context.GetValue("Model").ToStringValue());
        Assert.Null(context.GetValue("ViewLocalizer").ToObjectValue());
    }

    [Fact]
    public async Task InvokeInScopeAsync_NestedCallbackThrows_RestoresParentScope()
    {
        using var services = CreateServiceProvider();
        var context = new LiquidTemplateContext(services, new TemplateOptions());
        var viewContext = services.GetRequiredService<ViewContextAccessor>().ViewContext;
        context.SetValue("Model", "Root");

        var result = await context.InvokeInScopeAsync(viewContext, "Outer", async () =>
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                context.InvokeInScopeAsync<string>(viewContext, "Inner", () => throw new InvalidOperationException()));
            Assert.Equal("Outer", context.GetValue("Model").ToStringValue());
            Assert.Same(services.GetRequiredService<IViewLocalizer>(), context.GetValue("ViewLocalizer").ToObjectValue());
            return 42;
        });

        Assert.Equal(42, result);
        Assert.Equal("Root", context.GetValue("Model").ToStringValue());
        Assert.Null(context.GetValue("ViewLocalizer").ToObjectValue());
    }

    [Fact]
    public async Task InvokeInScopeAsync_NoViewContext_InitializesWithoutViewScope()
    {
        using var services = CreateServiceProvider();
        var context = new LiquidTemplateContext(services, new TemplateOptions());
        context.SetValue("Model", "Outer");

        await context.InvokeInScopeAsync(viewContext: null, model: "Inner", () =>
        {
            Assert.True(context.IsInitialized);
            Assert.Equal("Outer", context.GetValue("Model").ToStringValue());
            Assert.Null(context.GetValue("ViewLocalizer").ToObjectValue());
            return Task.CompletedTask;
        });

        Assert.Null(context.ViewContext);
        Mock.Get(services.GetRequiredService<IViewLocalizer>())
            .As<IViewContextAware>()
            .Verify(localizer => localizer.Contextualize(It.IsAny<ViewContext>()), Times.Never);
    }

    private static async Task<string> RenderAsync(IFluidTemplate template, LiquidTemplateContext context, bool useWriter)
    {
        if (!useWriter)
        {
            return await LiquidViewTemplateExtensions.RenderAsync(template, NullEncoder.Default, context, "Inner");
        }

        using var writer = new StringWriter();
        await LiquidViewTemplateExtensions.RenderAsync(template, writer, NullEncoder.Default, context, "Inner");
        return writer.ToString();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        services.AddSingleton(new ViewContextAccessor { ViewContext = new ViewContext() });
        var localizer = new Mock<IViewLocalizer>();
        localizer.As<IViewContextAware>();
        localizer.Setup(value => value.GetString("Name", It.IsAny<object[]>()))
            .Returns(new LocalizedString("Name", "Localized name"));
        services.AddSingleton(localizer.Object);
        var localClock = new Mock<ILocalClock>();
        localClock.Setup(clock => clock.GetLocalNowAsync()).ReturnsAsync(DateTimeOffset.UtcNow);
        localClock.Setup(clock => clock.GetLocalTimeZoneAsync())
            .ReturnsAsync(Mock.Of<ITimeZone>(timeZone => timeZone.TimeZoneId == "UTC"));
        services.AddSingleton(localClock.Object);
        return services.BuildServiceProvider();
    }
}
