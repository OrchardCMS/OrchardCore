using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Recipes;

/// <summary>
/// The admin list of recipes rendered by the <c>AdminList</c> shape. The page renders one list per feature,
/// all sharing this name so a column provider configures them together.
/// </summary>
public static class RecipesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Recipes</c> and <c>AdminListCell__Recipes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Recipes";

    /// <summary>
    /// The default columns used by layouts with columns, e.g. <c>Table</c>. There is no selection column:
    /// a recipe is run one at a time, the page has no bulk actions.
    /// </summary>
    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        new()
        {
            // The name and the description take the space left by the other columns.
            Name = "Name",
            Position = "10",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        },
        new()
        {
            Name = "Tags",
            Position = "20",
            Title = S["Tags"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            Name = "Actions",
            Position = "end",
            Title = S["Actions"],
            Zones = ["Actions", "ActionsMenu"],
            Width = AdminListColumn.AutoWidth,
            Alignment = AdminListColumnAlignment.End,
        },
    ];
}
