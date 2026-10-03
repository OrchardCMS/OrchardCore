using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Drivers;

/// <summary>
/// Builds the row of the recipes admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="RecipesAdminList"/> renders.
/// </summary>
public sealed class RecipeEntryDisplayDriver : DisplayDriver<RecipeEntry>
{
    public override Task<IDisplayResult> DisplayAsync(RecipeEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("RecipeEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("RecipeEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5"),
            View("RecipeEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("RecipeEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
