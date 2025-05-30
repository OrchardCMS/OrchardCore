using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;

    public IndexEntityDisplayDriver(
        IIndexEntityStore indexEntityStore,
        IServiceProvider serviceProvider,
        IStringLocalizer<IndexEntityDisplayDriver> stringLocalizer)
    {
        _indexEntityStore = indexEntityStore;
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(IndexEntity index, BuildDisplayContext context)
    {
        return CombineAsync(
            View("IndexEntity_Fields_SummaryAdmin", index)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("IndexEntity_Buttons_SummaryAdmin", index)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5"),
            View("IndexEntity_DefaultTags_SummaryAdmin", index)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("IndexEntity_DefaultMeta_SummaryAdmin", index)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5"),
            View("IndexEntity_ActionsMenuItems_SummaryAdmin", index)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
        );
    }

    public override IDisplayResult Edit(IndexEntity index, BuildEditorContext context)
    {
        return Initialize<EditIndexEntityViewModel>("IndexEntityFields_Edit", model =>
        {
            model.DisplayText = index.DisplayText;
            model.IndexName = index.IndexName;
            model.IsNew = context.IsNew;
        }).Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexEntity index, UpdateEditorContext context)
    {
        var model = new EditIndexEntityViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (context.IsNew)
        {
            var hasIndexName = !string.IsNullOrEmpty(model.IndexName);

            if (!hasIndexName)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["The index name is a required field."]);
            }
            else if (await _indexEntityStore.FindByNameAndProviderAsync(model.IndexName, index.ProviderName) is not null)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["There is already another index with the same name."]);
            }

            if (hasIndexName && !string.IsNullOrEmpty(index.ProviderName))
            {
                var nameProvider = _serviceProvider.GetKeyedService<IIndexNameProvider>(index.ProviderName);

                if (nameProvider is not null)
                {
                    index.IndexFullName = nameProvider.GetFullIndexName(model.IndexName);
                }
            }

            index.IndexName = model.IndexName;
        }

        if (string.IsNullOrEmpty(model.DisplayText))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.DisplayText), S["The display-text is required field."]);
        }

        index.DisplayText = model.DisplayText;

        return Edit(index, context);
    }
}
