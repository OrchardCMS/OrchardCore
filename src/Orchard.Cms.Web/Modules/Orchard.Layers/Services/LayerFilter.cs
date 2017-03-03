using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Modules.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Orchard.Admin;
using Orchard.ContentManagement.Display;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.Environment.Cache;
using Orchard.Scripting;

namespace Orchard.Layers.Services
{
    public class LayerFilter : IAsyncResultFilter
    {
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IUpdateModelAccessor _modelUpdaterAccessor;
        private readonly IScriptingManager _scriptingManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISignal _signal;
		private readonly ILayerService _layerService;
		private readonly IShapeFactory _shapeFactory;

		public LayerFilter(
			ILayerService layerService,
			IShapeFactory shapeFactory,
            ILayoutAccessor layoutAccessor,
            IContentItemDisplayManager contentItemDisplayManager,
            IUpdateModelAccessor modelUpdaterAccessor,
            IScriptingManager scriptingManager,
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache,
            ISignal signal)
        {
			_layerService = layerService;
			_layoutAccessor = layoutAccessor;
            _contentItemDisplayManager = contentItemDisplayManager;
            _modelUpdaterAccessor = modelUpdaterAccessor;
            _scriptingManager = scriptingManager;
            _serviceProvider = serviceProvider;
            _signal = signal;
			_shapeFactory = shapeFactory;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
			// Should only run on the front-end for a full view
			if (context.Result is ViewResult && !AdminAttribute.IsApplied(context.HttpContext))
			{
				var widgets = await _layerService.GetLayerWidgetsAsync(x => x.Published);
				var layers = (await _layerService.GetLayersAsync()).Layers.ToDictionary(x => x.Name);

				var layout = _layoutAccessor.GetLayout();
				var updater = _modelUpdaterAccessor.ModelUpdater;

				var engine = _scriptingManager.GetScriptingEngine("js");
				var scope = engine.CreateScope(_scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods()), _serviceProvider);

				var layersCache = new Dictionary<string, bool>();
				
				foreach (var widget in widgets)
				{
					var layer = layers[widget.Layer];

					if (layer == null)
					{
						continue;
					}

					bool display;
					if (!layersCache.TryGetValue(layer.Name, out display))
					{
						if (String.IsNullOrEmpty(layer.Rule))
						{
							display = false;
						}
						else
						{
							display = Convert.ToBoolean(engine.Evaluate(scope, layer.Rule));
						}

						layersCache[layer.Rule] = display;
					}

					if (!display)
					{
						continue;
					}

					IShape widgetContent = await _contentItemDisplayManager.BuildDisplayAsync(widget.ContentItem, updater);
						
					widgetContent.Classes.Add("widget");
					widgetContent.Classes.Add("widget-" + widget.ContentItem.ContentType.HtmlClassify());

					var wrapper = _shapeFactory.Create("Widget_Wrapper", new { Widget = widget.ContentItem, Content = widgetContent });
					wrapper.Metadata.Alternates.Add("Widget_Wrapper__" + widget.ContentItem.ContentType);

					var contentZone = layout.Zones[widget.Zone];
					contentZone.Add(wrapper);
				}
				
			}

			await next.Invoke();
        }
    }
}
