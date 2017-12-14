using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;

namespace OrchardCore.Layers.Drivers
{
	public class LayerMetadataWelder : ContentDisplayDriver
    {
		private readonly ILayerService _layerService;

		public LayerMetadataWelder(ILayerService layerService)
		{
			_layerService = layerService;
		}

		public override async Task<IDisplayResult> EditAsync(ContentItem model, BuildEditorContext context)
		{
			var layerMetadata = model.As<LayerMetadata>();

			if (layerMetadata == null)
			{
				layerMetadata = new LayerMetadata();

				// Are we loading an editor that requires layer metadata?
				if (await context.Updater.TryUpdateModelAsync(layerMetadata, "LayerMetadata", m => m.Zone, m => m.Position)
					&& !String.IsNullOrEmpty(layerMetadata.Zone))
				{
					model.Weld(layerMetadata);
				}
				else
				{
					return null;
				}
			}

			return Shape<LayerMetadataEditViewModel>("LayerMetadata_Edit", async shape =>
			{
				shape.LayerMetadata = layerMetadata;
				shape.Layers = (await _layerService.GetLayersAsync()).Layers;
			})
			.Location("Content:before");
		}

        public override async Task<IDisplayResult> UpdateAsync(ContentItem model, UpdateEditorContext context)
        {
            var layerMetadata = new LayerMetadata();

            if (!await context.Updater.TryUpdateModelAsync(layerMetadata, "LayerMetadata", m => m.Zone, m => m.Position, m => m.RenderTitle, m => m.Title, m => m.Layer)
            || String.IsNullOrEmpty(layerMetadata.Zone)) {
                return null;
            }

			model.Apply(layerMetadata);

			return await EditAsync(model, context);
        }
    }
}
