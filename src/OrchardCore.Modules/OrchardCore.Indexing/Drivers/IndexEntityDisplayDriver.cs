using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Indexing.Drivers;

internal sealed class IndexEntityDisplayDriver : DisplayDriver<IndexEntity>
{
    private readonly IIndexEntityStore _indexEntityStore;

    internal readonly IStringLocalizer S;

    public IndexEntityDisplayDriver(
        IIndexEntityStore indexEntityStore,
        IStringLocalizer<IndexEntityDisplayDriver> stringLocalizer)
    {
        _indexEntityStore = indexEntityStore;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(IndexEntity entity, BuildDisplayContext context)
    {
        return CombineAsync(
            View("IndexEntity_Fields_SummaryAdmin", entity)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("IndexEntity_Buttons_SummaryAdmin", entity)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5"),
            View("IndexEntity_DefaultTags_SummaryAdmin", entity)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("IndexEntity_DefaultMeta_SummaryAdmin", entity)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5"),
            View("IndexEntity_ActionsMenuItems_SummaryAdmin", entity)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
        );
    }

    public override IDisplayResult Edit(IndexEntity entity, BuildEditorContext context)
    {
        return Initialize<EditIndexEntityViewModel>("IndexEntityFields_Edit", model =>
        {
            model.DisplayText = entity.DisplayText;
            model.IndexName = entity.IndexName;
            model.IsNew = context.IsNew;
        }).Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexEntity entity, UpdateEditorContext context)
    {
        var model = new EditIndexEntityViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (context.IsNew)
        {
            if (string.IsNullOrEmpty(model.IndexName))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["The index name is a required field."]);
            }
            else if (await _indexEntityStore.FindByNameAndProviderAsync(model.IndexName, entity.ProviderName) is not null)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["There is already another index with the same name."]);
            }

            entity.IndexName = model.IndexName;
        }

        if (string.IsNullOrEmpty(model.DisplayText))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.DisplayText), S["The display-text is required field."]);
        }

        entity.DisplayText = model.DisplayText;

        return Edit(entity, context);
    }
}
