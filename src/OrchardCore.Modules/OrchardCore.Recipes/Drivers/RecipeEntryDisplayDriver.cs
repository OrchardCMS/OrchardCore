using OrchardCore.DisplayManagement.Shapes;
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
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape rowShape)
        {
            rowShape.Attributes["data-filter-value"] = entry.Feature + " " + entry.DisplayName;
        }

        return CombineAsync(
            View("RecipeEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("RecipeEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:5"),
            View("RecipeEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Tags:5"),
            View("RecipeEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
        );
    }
}
