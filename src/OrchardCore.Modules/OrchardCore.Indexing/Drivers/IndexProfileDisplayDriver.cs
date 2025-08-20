using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Indexing.Drivers;

internal sealed class IndexProfileDisplayDriver : DisplayDriver<IndexProfile>
{
    private readonly IIndexProfileStore _indexProfileStore;
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;

    public IndexProfileDisplayDriver(
        IIndexProfileStore indexProfileStore,
        IServiceProvider serviceProvider,
        IStringLocalizer<IndexProfileDisplayDriver> stringLocalizer)
    {
        _indexProfileStore = indexProfileStore;
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(IndexProfile indexProfile, BuildDisplayContext context)
    {
        return CombineAsync(
            View("IndexProfile_Fields_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("IndexProfile_Buttons_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5"),
            View("IndexProfile_DefaultTags_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("IndexProfile_DefaultMeta_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5"),
            View("IndexProfile_ActionsMenuItems_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
        );
    }

    public override IDisplayResult Edit(IndexProfile indexProfile, BuildEditorContext context)
    {
        return Initialize<EditIndexProfileViewModel>("IndexProfileFields_Edit", model =>
        {
            model.Name = indexProfile.Name;
            model.IndexName = indexProfile.IndexName;
            model.IsNew = context.IsNew;
        }).Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexProfile indexProfile, UpdateEditorContext context)
    {
        var model = new EditIndexProfileViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (context.IsNew)
        {
            var hasIndexName = !string.IsNullOrEmpty(model.IndexName);

            if (!hasIndexName)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["The index name is a required field."]);
            }
            else if (await _indexProfileStore.FindByIndexNameAndProviderAsync(model.IndexName, indexProfile.ProviderName) is not null)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["There is already another index with the same name."]);
            }

            if (hasIndexName && model.IndexName.Length > 255)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["The index name must be less than 255 characters."]);
            }

            if (hasIndexName && !string.IsNullOrEmpty(indexProfile.ProviderName))
            {
                var nameProvider = _serviceProvider.GetKeyedService<IIndexNameProvider>(indexProfile.ProviderName);

                if (nameProvider is not null)
                {
                    indexProfile.IndexFullName = nameProvider.GetFullIndexName(model.IndexName);
                }
            }

            indexProfile.IndexName = model.IndexName;
        }

        if (string.IsNullOrEmpty(model.Name))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["The name is required field."]);
        }
        else
        {
            if (model.Name.Length > 255)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["The name must be less than 255 characters."]);
            }
            else
            {
                var existing = await _indexProfileStore.FindByNameAsync(model.Name);

                if (existing is not null && existing.Id != indexProfile.Id)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["There is already another index with the same name."]);
                }
            }
        }

        indexProfile.Name = model.Name;

        return Edit(indexProfile, context);
    }
}
