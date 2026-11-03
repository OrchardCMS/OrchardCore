using OrchardCore.DisplayManagement.Shapes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Indexing.Drivers;

internal sealed class IndexProfileDisplayDriver : DisplayDriver<IndexProfile>
{
    private readonly IndexProfileIdentityValidator _identities;
    private readonly IServiceProvider _serviceProvider;

    public IndexProfileDisplayDriver(
        IndexProfileIdentityValidator identities,
        IServiceProvider serviceProvider)
    {
        _identities = identities;
        _serviceProvider = serviceProvider;
    }

    public override Task<IDisplayResult> DisplayAsync(IndexProfile indexProfile, BuildDisplayContext context)
    {
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape rowShape)
        {
            rowShape.Classes.Add("item");
            rowShape.Attributes["data-filter-value"] = indexProfile.Name?.ToLowerInvariant() ?? string.Empty;
        }

        return CombineAsync(
            View("IndexProfile_Checkbox_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"),
            View("IndexProfile_Fields_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("IndexProfile_Buttons_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
            View("IndexProfile_DefaultTags_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Source:5"),
            View("IndexProfile_DefaultMeta_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Modified:5"),
            View("IndexProfile_ActionsMenuItems_SummaryAdmin", indexProfile)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:5")
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

        indexProfile.Name = model.Name;
        foreach (var error in await _identities.ValidateAsync(indexProfile))
        {
            foreach (var member in error.MemberNames)
            {
                context.Updater.ModelState.AddModelError(Prefix, member, error.ErrorMessage);
            }
        }

        return Edit(indexProfile, context);
    }
}
