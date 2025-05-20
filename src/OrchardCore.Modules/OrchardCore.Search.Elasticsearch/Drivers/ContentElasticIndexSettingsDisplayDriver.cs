using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Localization;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Drivers;

internal sealed class ContentElasticIndexSettingsDisplayDriver : DisplayDriver<ElasticIndexSettings>
{
    internal readonly IStringLocalizer S;

    public ContentElasticIndexSettingsDisplayDriver(IStringLocalizer<ContentElasticIndexSettingsDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ElasticIndexSettings settings, BuildEditorContext context)
    {
        if (!string.Equals(ElasticsearchConstants.ContentsIndexSource, settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Initialize<ContentIndexMetadataViewModel>("ContentIndexMetadata_Edit", model =>
        {
            var metadata = settings.As<ContentIndexMetadata>();

            model.IndexLatest = metadata.IndexLatest;
            model.IndexedContentTypes = metadata.IndexedContentTypes;
            model.Culture = metadata.Culture;
            model.StoreSourceData = metadata.StoreSourceData;
            model.Cultures = ILocalizationService.GetAllCulturesAndAliases()
            .Select(culture => new SelectListItem
            {
                Text = $"{culture.Name} ({culture.DisplayName})",
                Value = culture.Name,
            });

        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(ElasticIndexSettings settings, UpdateEditorContext context)
    {
        if (!string.Equals(ElasticsearchConstants.ContentsIndexSource, settings.Source, StringComparison.OrdinalIgnoreCase))
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
            m.StoreSourceData = model.StoreSourceData;
            m.IndexLatest = model.IndexLatest;
            m.IndexedContentTypes = model.IndexedContentTypes ?? [];
            m.Culture = model.Culture;
        });

        return Edit(settings, context);
    }
}
