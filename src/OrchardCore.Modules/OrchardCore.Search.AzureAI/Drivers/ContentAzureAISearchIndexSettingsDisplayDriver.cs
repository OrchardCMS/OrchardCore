using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Localization;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Search.AzureAI.Drivers;

internal sealed class ContentAzureAISearchIndexSettingsDisplayDriver : DisplayDriver<AzureAISearchIndexSettings>
{
    internal readonly IStringLocalizer S;

    public ContentAzureAISearchIndexSettingsDisplayDriver(IStringLocalizer<ContentAzureAISearchIndexSettingsDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AzureAISearchIndexSettings settings, BuildEditorContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Initialize<ContentIndexMetadataViewModel>("ContentIndexMetadata_Edit", model =>
        {
            var metadata = settings.As<ContentIndexMetadata>();

            model.IndexLatest = metadata.IndexLatest;
            model.IndexedContentTypes = metadata.IndexedContentTypes;
            model.Culture = metadata.Culture;

            model.Cultures = ILocalizationService.GetAllCulturesAndAliases()
            .Select(culture => new SelectListItem
            {
                Text = $"{culture.Name} ({culture.DisplayName})",
                Value = culture.Name,
            });

        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchIndexSettings settings, UpdateEditorContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var model = new ContentIndexMetadataViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.IndexedContentTypes is null || model.IndexedContentTypes.Length == 0)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexedContentTypes), S["At least one content type must be selected."]);
        }

        settings.Alter<ContentIndexMetadata>(m =>
        {
            m.IndexLatest = model.IndexLatest;
            m.IndexedContentTypes = model.IndexedContentTypes ?? [];
            m.Culture = model.Culture;
        });

        return Edit(settings, context);
    }
}
