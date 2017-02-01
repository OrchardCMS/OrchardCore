using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Orchard.Admin;
using Orchard.ContentManagement.Display;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.Environment.Cache;
using Orchard.Scripting;
using Orchard.Utility;

namespace Orchard.Layers.Services
{
	public class LayerFilter : IAsyncResultFilter
    {
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IModelUpdaterAccessor _modelUpdaterAccessor;
        private readonly IScriptingManager _scriptingManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISignal _signal;
		private readonly ILayerService _layerService;

		public LayerFilter(
			ILayerService layerService,
            ILayoutAccessor layoutAccessor,
            IContentItemDisplayManager contentItemDisplayManager,
            IModelUpdaterAccessor modelUpdaterAccessor,
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
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
			// Should only run on the front-end for a full view
			if (context.Result is ViewResult && !AdminAttribute.IsApplied(context.HttpContext))
			{
				var widgets = await _layerService.GetLayerWidgetsAsync(x => x.Published);
				var layers = await _layerService.GetLayersAsync();

				var layout = _layoutAccessor.GetLayout();
				var updater = _modelUpdaterAccessor.ModelUpdater;

				var engine = _scriptingManager.GetScriptingEngine("js");
				var scope = engine.CreateScope(_scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods()), _serviceProvider);

				foreach (var layer in layers.Layers)
				{
					if (String.IsNullOrEmpty(layer.Rule))
					{
						continue;
					}

					var display = Convert.ToBoolean(engine.Evaluate(scope, layer.Rule));

					if (!display)
					{
						continue;
					}

					foreach (var widget in widgets)
					{
						if (widget.Layer != layer.Name) continue;

						var widgetContent = await _contentItemDisplayManager.BuildDisplayAsync(widget.ContentItem, updater);

						widgetContent.Classes.Add("widget");
						widgetContent.Classes.Add("widget-" + widget.ContentItem.ContentType.HtmlClassify());

						var contentZone = layout.Zones[widget.Zone];
						contentZone.Add(widgetContent);
					}
				}
			}

			await next.Invoke();
        }
    }
}
