using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Admin;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Documents;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Documents;
using OrchardCore.Layers.Handlers;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Rules;

namespace OrchardCore.Layers.Services
{
    public class LayerFilter : IAsyncResultFilter
    {
        private const string WidgetsKey = "OrchardCore.Layers.LayerFilter:AllWidgets";
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IUpdateModelAccessor _modelUpdaterAccessor;
        private readonly IRuleService _ruleService;
        private readonly IMemoryCache _memoryCache;
        private readonly IThemeManager _themeManager;
        private readonly IAdminThemeService _adminThemeService;
        private readonly ILayerService _layerService;
        private readonly IVolatileDocumentManager<LayerState> _layerStateManager;

        public LayerFilter(
            IContentDefinitionManager contentDefinitionManager,
            ILayerService layerService,
            ILayoutAccessor layoutAccessor,
            IContentItemDisplayManager contentItemDisplayManager,
            IUpdateModelAccessor modelUpdaterAccessor,
            IRuleService ruleService,
            IMemoryCache memoryCache,
            IThemeManager themeManager,
            IAdminThemeService adminThemeService,
            IVolatileDocumentManager<LayerState> layerStateManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _layerService = layerService;
            _layoutAccessor = layoutAccessor;
            _contentItemDisplayManager = contentItemDisplayManager;
            _modelUpdaterAccessor = modelUpdaterAccessor;
            _ruleService = ruleService;
            _memoryCache = memoryCache;
            _themeManager = themeManager;
            _adminThemeService = adminThemeService;
            _layerStateManager = layerStateManager;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Should only run on the front-end for a full view
            if ((context.Result is ViewResult || context.Result is PageResult) &&
                !AdminAttribute.IsApplied(context.HttpContext))
            {
                // Even if the Admin attribute is not applied we might be using the admin theme, for instance in Login views.
                // In this case don't render Layers.
                var selectedTheme = (await _themeManager.GetThemeAsync())?.Id;
                var adminTheme = await _adminThemeService.GetAdminThemeNameAsync();
                if (selectedTheme == adminTheme)
                {
                    await next.Invoke();
                    return;
                }

                var layerState = await _layerStateManager.GetOrCreateImmutableAsync();

                if (!_memoryCache.TryGetValue<CacheEntry>(WidgetsKey, out var cacheEntry) || cacheEntry.Identifier != layerState.Identifier)
                {
                    cacheEntry = new CacheEntry()
                    {
                        Identifier = layerState.Identifier,
                        Widgets = await _layerService.GetLayerWidgetsMetadataAsync(x => x.Published)
                    };

                    _memoryCache.Set(WidgetsKey, cacheEntry);
                }

                var widgets = cacheEntry.Widgets;

                var layers = (await _layerService.GetLayersAsync()).Layers.ToDictionary(x => x.Name);

                var layout = await _layoutAccessor.GetLayoutAsync();
                var updater = _modelUpdaterAccessor.ModelUpdater;

                var layersCache = new Dictionary<string, bool>();
                var contentDefinitions = _contentDefinitionManager.ListTypeDefinitions();

                foreach (var widget in widgets)
                {
                    var layer = widget.Layer != null && layers.TryGetValue(widget.Layer, out var widgetLayer) ? widgetLayer : null;

                    if (layer == null)
                    {
                        continue;
                    }

                    bool display;
                    if (!layersCache.TryGetValue(layer.Name, out display))
                    {
                        display = await _ruleService.EvaluateAsync(layer.LayerRule);

                        layersCache[layer.Name] = display;
                    }

                    if (!display)
                    {
                        continue;
                    }

                    if (contentDefinitions.Any(c => c.Name == widget.ContentItem.ContentType))
                    {
                        var widgetContent = await _contentItemDisplayManager.BuildDisplayAsync(widget.ContentItem, updater);

                        widgetContent.Classes.Add("widget");
                        widgetContent.Classes.Add("widget-" + widget.ContentItem.ContentType.HtmlClassify());

                        var wrapper = new WidgetWrapper
                        {
                            Widget = widget.ContentItem,
                            Content = widgetContent
                        };

                        wrapper.Metadata.Alternates.Add("Widget_Wrapper__" + widget.ContentItem.ContentType);
                        wrapper.Metadata.Alternates.Add("Widget_Wrapper__Zone__" + widget.Zone);

                        var contentZone = layout.Zones[widget.Zone];

                        await contentZone.AddAsync(wrapper, "");
                    }
                }
            }

            await next.Invoke();
        }

        internal class CacheEntry : Document
        {
            public IEnumerable<LayerMetadata> Widgets { get; set; }
        }
    }
}
