using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes;

/// <summary>
/// Declares the columns of the <see cref="RecipesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. There is no selection column: a recipe is run one at a time, the page has no bulk actions.
/// </summary>
public sealed class RecipesAdminListColumnDriver : AdminListColumnDriver<RecipeEntry>
{
    private readonly IStringLocalizer S;

    public RecipesAdminListColumnDriver(IStringLocalizer<RecipesAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(new AdminListColumn
        {
            // The name and the description take the space left by the other columns.
            Name = "Name",
            Position = "10",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Tags",
            Position = "20",
            Title = S["Tags"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
