using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.ViewModels;
using OrchardCore.Localization;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Indexing.Drivers;

internal sealed class ContentIndexEntityDisplayDriver : DisplayDriver<IndexEntity>
{
    internal readonly IStringLocalizer S;

    public ContentIndexEntityDisplayDriver(IStringLocalizer<ContentIndexEntityDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(IndexEntity entity, BuildEditorContext context)
    {
        if (!string.Equals(IndexingConstants.ContentsIndexSource, entity.Type, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Initialize<ContentIndexMetadataViewModel>("ContentIndexEntityMetadata_Edit", model =>
        {
            var metadata = entity.As<ContentIndexEntityMetadata>();

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

    public override async Task<IDisplayResult> UpdateAsync(IndexEntity entity, UpdateEditorContext context)
    {
        if (!string.Equals(IndexingConstants.ContentsIndexSource, entity.Type, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var model = new ContentIndexMetadataViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.IndexedContentTypes is null || model.IndexedContentTypes.Length == 0)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexedContentTypes), S["At least one content type must be selected."]);
        }

        entity.Alter<ContentIndexEntityMetadata>(m =>
        {
            m.IndexLatest = model.IndexLatest;
            m.IndexedContentTypes = model.IndexedContentTypes ?? [];
            m.Culture = model.Culture;
        });

        return Edit(entity, context);
    }
}
