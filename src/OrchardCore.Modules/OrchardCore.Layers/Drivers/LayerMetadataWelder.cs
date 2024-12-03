using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Layers.Drivers;

public sealed class LayerMetadataWelder : ContentDisplayDriver
{
    private readonly ILayerService _layerService;

    internal readonly IStringLocalizer S;

    public LayerMetadataWelder(
        ILayerService layerService,
        IStringLocalizer<LayerMetadataWelder> stringLocalizer)
    {
        _layerService = layerService;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ContentItem model, BuildEditorContext context)
    {
        if (!model.TryGet<LayerMetadata>(out var layerMetadata))
        {
            layerMetadata = new LayerMetadata();
            await context.Updater.TryUpdateModelAsync(layerMetadata, string.Empty, m => m.Zone, m => m.Position);

            // Are we loading an editor that requires layer metadata?
            if (string.IsNullOrEmpty(layerMetadata.Zone))
            {
                return null;
            }

            model.Weld(layerMetadata);
        }

        return Initialize<LayerMetadataEditViewModel>("LayerMetadata_Edit", async m =>
        {
            m.Title = model.DisplayText;
            m.RenderTitle = layerMetadata.RenderTitle;
            m.Position = layerMetadata.Position;
            m.Zone = layerMetadata.Zone;
            m.Layer = layerMetadata.Layer;
            m.Layers = (await _layerService.GetLayersAsync()).Layers
            .Select(x => new SelectListItem(x.Name, x.Name));
        })
        .Location("Content:before");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentItem model, UpdateEditorContext context)
    {
        var viewModel = new LayerMetadataEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        if (string.IsNullOrEmpty(viewModel.Title))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Title), S["The Title field is required"]);
        }

        if (string.IsNullOrEmpty(viewModel.Zone))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Zone), S["The Zone field is required"]);
        }

        if (string.IsNullOrEmpty(viewModel.Layer))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Layer), S["The Layer field is required"]);
        }
        else
        {
            var document = await _layerService.GetLayersAsync();

            if (!document.Layers.Any(x => x.Name == viewModel.Layer))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Layer), S["Invalid Layer value provided"]);
            }
        }

        model.Apply(new LayerMetadata
        {
            RenderTitle = viewModel.RenderTitle,
            Zone = viewModel.Zone,
            Position = viewModel.Position,
            Layer = viewModel.Layer,
        });

        model.DisplayText = viewModel.Title;

        return await EditAsync(model, context);
    }
}
