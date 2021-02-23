using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Layers.Drivers
{
    public class AdminLayerMetadataWelder : ContentDisplayDriver
    {
        private readonly IAdminLayerService _layerService;
        private readonly IStringLocalizer S;

        public const string LayerPrefix = "AdminLayerMetadata";

        public AdminLayerMetadataWelder(IAdminLayerService layerService, IStringLocalizer<AdminLayerMetadataWelder> stringLocalizer)
        {
            _layerService = layerService;
            S = stringLocalizer;
        }

        protected override void BuildPrefix(ContentItem model, string htmlFieldPrefix)
        {
            base.BuildPrefix(model, htmlFieldPrefix);
            if (string.IsNullOrWhiteSpace(htmlFieldPrefix))
            {
                Prefix = LayerPrefix;
            }
        }

        public override async Task<IDisplayResult> EditAsync(ContentItem model, BuildEditorContext context)
        {
            var layerMetadata = model.As<AdminLayerMetadata>();

            if (layerMetadata == null)
            {
                layerMetadata = new AdminLayerMetadata();

                // Are we loading an editor that requires layer metadata?
                if (await context.Updater.TryUpdateModelAsync(layerMetadata, Prefix, m => m.Zone, m => m.Position)
                    && !String.IsNullOrEmpty(layerMetadata.Zone))
                {
                    model.Weld(layerMetadata);
                }
                else
                {
                    return null;
                }
            }

            return Initialize<AdminLayerMetadataEditViewModel>("AdminLayerMetadata_Edit", async shape =>
            {
                shape.Title = model.DisplayText;
                shape.LayerMetadata = layerMetadata;
                shape.Layers = (await _layerService.GetLayersAsync()).Layers;
            })
            .Location("Content:before");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem model, UpdateEditorContext context)
        {
            var viewModel = new AdminLayerMetadataEditViewModel();

            await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

            if (viewModel.LayerMetadata == null)
            {
                return null;
            }

            if (String.IsNullOrEmpty(viewModel.LayerMetadata.Zone))
            {
                context.Updater.ModelState.AddModelError(Prefix, "AdminLayerMetadata.Zone", S["Zone is missing"]);
            }

            if (String.IsNullOrEmpty(viewModel.LayerMetadata.Layer))
            {
                context.Updater.ModelState.AddModelError(Prefix, "AdminLayerMetadata.Layer", S["Layer is missing"]);
            }

            if (context.Updater.ModelState.IsValid)
            {
                model.Apply(viewModel.LayerMetadata);
            }

            model.DisplayText = viewModel.Title;

            return await EditAsync(model, context);
        }
    }
}
