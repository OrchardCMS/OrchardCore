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
    private readonly ILayerWidgetService _widgets;

    internal readonly IStringLocalizer S;

    public LayerMetadataWelder(
        ILayerService layerService,
        ILayerWidgetService widgets,
        IStringLocalizer<LayerMetadataWelder> stringLocalizer)
    {
        _layerService = layerService;
        _widgets = widgets;
        S = stringLocalizer;
    }

    protected override void BuildPrefix(ContentItem model, string htmlFieldPrefix)
    {
        base.BuildPrefix(model, htmlFieldPrefix);
        if (string.IsNullOrWhiteSpace(htmlFieldPrefix))
        {
            Prefix = "LayerMetadata";
        }
    }

    public override async Task<IDisplayResult> EditAsync(ContentItem model, BuildEditorContext context)
    {
        if (!model.TryGet<LayerMetadata>(out var layerMetadata))
        {
            layerMetadata = new LayerMetadata();
            await context.Updater.TryUpdateModelAsync(layerMetadata, Prefix, m => m.Zone, m => m.Position);

            // Are we loading an editor that requires layer metadata?
            if (!string.IsNullOrEmpty(layerMetadata.Zone))
            {
                model.Weld(layerMetadata);
            }
            else
            {
                return null;
            }
        }

        return Initialize<LayerMetadataEditViewModel>("LayerMetadata_Edit", async shape =>
        {
            shape.Title = model.DisplayText;
            shape.LayerMetadata = layerMetadata;
            shape.Layers = (await _layerService.GetLayersAsync()).Layers;
        }).Location("Content:before");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentItem model, UpdateEditorContext context)
    {
        var viewModel = new LayerMetadataEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        if (viewModel.LayerMetadata == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(viewModel.Title))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Title), S["Title is required field."]);
        }

        foreach (var error in await _widgets.ValidateAsync(viewModel.LayerMetadata))
        {
            foreach (var message in error.Value)
            {
                var field = char.ToUpperInvariant(error.Key[0]) + error.Key[1..];
                context.Updater.ModelState.AddModelError(Prefix, "LayerMetadata." + field, message);
            }
        }
        if (!string.IsNullOrEmpty(viewModel.LayerMetadata.Layer)
            && await _layerService.GetLayerAsync(viewModel.LayerMetadata.Layer) is { } layer)
        {
            viewModel.LayerMetadata.Layer = layer.Name;
        }

        model.Apply(viewModel.LayerMetadata);

        model.DisplayText = viewModel.Title;

        return await EditAsync(model, context);
    }
}
