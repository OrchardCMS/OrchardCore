using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Documents;
using OrchardCore.Environment.Extensions;
using OrchardCore.Layers.Handlers;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Rules;
using OrchardCore.Rules.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Layers;

public class LayerFilterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OnResultExecutionAsync_DownstreamFailure_DoesNotDuplicateWidgets(bool newFilterInstance)
    {
        using var test = new LayerFilterContext(
            CreateWidget("first", "Footer", "Always"),
            CreateWidget("second", "Footer", "Always"));
        var unrelated = new Shape();
        await test.Layout.Zones["Footer"].AddAsync(unrelated, "0");
        var filter = test.CreateFilter();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            filter.OnResultExecutionAsync(test.ResultContext, () => throw new InvalidOperationException("Before layout rendering")));
        Assert.Equal(3, test.Layout.Zones["Footer"].Items.Count);

        test.HttpContext.Request.Path = "/Error";
        if (newFilterInstance)
        {
            filter = test.CreateFilter();
        }

        await filter.OnResultExecutionAsync(test.ResultContext, test.Next);
        await filter.OnResultExecutionAsync(test.ResultContext, test.Next);

        var items = test.Layout.Zones["Footer"].Items;
        Assert.Equal(3, items.Count);
        Assert.Contains(unrelated, items);
        Assert.Equal(["first", "second"], items.OfType<WidgetWrapper>().Select(wrapper => wrapper.Widget.ContentItemId));
    }

    [Fact]
    public async Task OnResultExecutionAsync_PathChanges_ReplacesStaleWidgetsAndPreservesUnrelatedContent()
    {
        using var test = new LayerFilterContext(
            CreateWidget("original", "Header", "Original"),
            CreateWidget("error", "Footer", "Error"),
            CreateWidget("always", "Footer", "Always"));
        var headerContent = new Shape();
        var footerContent = new WidgetWrapper();
        await test.Layout.Zones["Header"].AddAsync(headerContent, "0");
        await test.Layout.Zones["Footer"].AddAsync(footerContent, "0");
        test.RuleService
            .Setup(service => service.EvaluateAsync(It.IsAny<Rule>()))
            .Returns((Rule rule) => ValueTask.FromResult(
                rule == test.Layers.Single(layer => layer.Name == "Always").LayerRule ||
                rule == test.Layers.Single(layer => layer.Name ==
                    (test.HttpContext.Request.Path == "/Error" ? "Error" : "Original")).LayerRule));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            test.CreateFilter().OnResultExecutionAsync(test.ResultContext, () => throw new InvalidOperationException("Before layout rendering")));
        Assert.Equal(2, test.Layout.Zones["Header"].Items.Count);

        test.HttpContext.Request.Path = "/Error";
        await test.CreateFilter().OnResultExecutionAsync(test.ResultContext, test.Next);

        Assert.Same(headerContent, Assert.Single(test.Layout.Zones["Header"].Items));
        var footerItems = test.Layout.Zones["Footer"].Items;
        Assert.Equal(3, footerItems.Count);
        Assert.Contains(footerContent, footerItems);
        Assert.Equal(["error", "always"], footerItems.OfType<WidgetWrapper>()
            .Where(wrapper => wrapper.Widget != null)
            .Select(wrapper => wrapper.Widget.ContentItemId));
    }

    [Fact]
    public async Task OnResultExecutionAsync_NoLayersMatch_RemovesPreviousWidgets()
    {
        using var test = new LayerFilterContext(CreateWidget("footer", "Footer", "Always"));
        await test.CreateFilter().OnResultExecutionAsync(test.ResultContext, test.Next);

        test.RuleService.Setup(service => service.EvaluateAsync(It.IsAny<Rule>())).ReturnsAsync(false);
        await test.CreateFilter().OnResultExecutionAsync(test.ResultContext, test.Next);

        Assert.Empty(test.Layout.Zones["Footer"].Items);
    }

    [Fact]
    public async Task OnResultExecutionAsync_AdminThemeSelected_RemovesPreviousWidgets()
    {
        using var test = new LayerFilterContext(CreateWidget("footer", "Footer", "Always"));
        await test.CreateFilter().OnResultExecutionAsync(test.ResultContext, test.Next);

        test.ThemeManager.Setup(manager => manager.GetThemeAsync())
            .ReturnsAsync(Mock.Of<IExtensionInfo>(theme => theme.Id == "Admin"));
        await test.CreateFilter().OnResultExecutionAsync(test.ResultContext, test.Next);

        Assert.Empty(test.Layout.Zones["Footer"].Items);
    }

    [Fact]
    public async Task OnResultExecutionAsync_WidgetBuildFails_RemovesPartialContributionsOnReexecution()
    {
        using var test = new LayerFilterContext(
            CreateWidget("first", "Footer", "Always"),
            CreateWidget("second", "Footer", "Always"));
        test.DisplayManager.SetupSequence(manager => manager.BuildDisplayAsync(
            It.IsAny<ContentItem>(), It.IsAny<IUpdateModel>(), "", ""))
            .ReturnsAsync(new Shape())
            .ThrowsAsync(new InvalidOperationException("Widget display failed"))
            .ReturnsAsync(new Shape())
            .ReturnsAsync(new Shape());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            test.CreateFilter().OnResultExecutionAsync(test.ResultContext, test.Next));
        Assert.Single(test.Layout.Zones["Footer"].Items);

        await test.CreateFilter().OnResultExecutionAsync(test.ResultContext, test.Next);

        Assert.Equal(["first", "second"], test.Layout.Zones["Footer"].Items
            .OfType<WidgetWrapper>().Select(wrapper => wrapper.Widget.ContentItemId));
    }

    private static LayerMetadata CreateWidget(string id, string zone, string layer)
    {
        var contentItem = new ContentItem
        {
            ContentItemId = id,
            ContentType = "TestWidget",
            Published = true,
        };
        var metadata = new LayerMetadata { Zone = zone, Layer = layer };
        contentItem.Weld(metadata);

        return metadata;
    }

    private sealed class LayerFilterContext : IDisposable
    {
        private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
        private readonly ServiceProvider _services;
        private readonly IContentDefinitionManager _definitionManager;
        private readonly ILayerService _layerService;
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IVolatileDocumentManager<LayerState> _layerStateManager;

        public DefaultHttpContext HttpContext { get; } = new();
        public ZoneHolding Layout { get; } = new(() => ValueTask.FromResult<IShape>(new Shape()));
        public Mock<IRuleService> RuleService { get; } = new();
        public Mock<IThemeManager> ThemeManager { get; } = new();
        public Mock<IContentItemDisplayManager> DisplayManager { get; } = new();
        public List<Layer> Layers { get; }
        public ResultExecutingContext ResultContext { get; }

        public LayerFilterContext(params LayerMetadata[] widgets)
        {
            _services = new ServiceCollection()
                .AddSingleton(Mock.Of<IAdminThemeService>(service => service.GetAdminThemeNameAsync() == Task.FromResult("Admin")))
                .BuildServiceProvider();
            HttpContext.RequestServices = _services;
            HttpContext.Request.Path = "/Original";
            var actionContext = new ActionContext(HttpContext, new RouteData(), new ActionDescriptor());
            ResultContext = new ResultExecutingContext(actionContext, [], new ViewResult(), controller: null);
            Layers = widgets.Select(widget => widget.Layer).Distinct()
                .Select(name => new Layer { Name = name, LayerRule = new Rule() }).ToList();

            var definition = new ContentTypeDefinitionBuilder().WithName("TestWidget").Stereotype("Widget").Build();
            _definitionManager = Mock.Of<IContentDefinitionManager>(manager =>
                manager.GetTypeDefinitionAsync("TestWidget") == Task.FromResult(definition));
            var layerService = new Mock<ILayerService>();
            layerService.Setup(service => service.GetLayerWidgetsMetadataAsync(It.IsAny<Expression<Func<ContentItemIndex, bool>>>()))
                .ReturnsAsync(widgets);
            layerService.Setup(service => service.GetLayersAsync()).ReturnsAsync(new LayersDocument { Layers = Layers });
            _layerService = layerService.Object;
            _layoutAccessor = Mock.Of<ILayoutAccessor>(accessor => accessor.GetLayoutAsync() == Task.FromResult<IZoneHolding>(Layout));
            _layerStateManager = Mock.Of<IVolatileDocumentManager<LayerState>>(manager =>
                manager.GetOrCreateImmutableAsync() == Task.FromResult(new LayerState()));
            RuleService.Setup(service => service.EvaluateAsync(It.IsAny<Rule>())).ReturnsAsync(true);
            ThemeManager.Setup(manager => manager.GetThemeAsync()).ReturnsAsync(Mock.Of<IExtensionInfo>(theme => theme.Id == "Theme"));
            DisplayManager.Setup(manager => manager.BuildDisplayAsync(It.IsAny<ContentItem>(), It.IsAny<IUpdateModel>(), "", ""))
                .ReturnsAsync(() => new Shape());
        }

        public LayerFilter CreateFilter() => new(
            _definitionManager,
            _layerService,
            _layoutAccessor,
            DisplayManager.Object,
            Mock.Of<IUpdateModelAccessor>(),
            RuleService.Object,
            _memoryCache,
            ThemeManager.Object,
            Mock.Of<IAuthorizationService>(),
            _layerStateManager);

        public Task<ResultExecutedContext> Next() => Task.FromResult(
            new ResultExecutedContext(ResultContext, [], ResultContext.Result, controller: null));

        public void Dispose()
        {
            _memoryCache.Dispose();
            _services.Dispose();
        }
    }
}
