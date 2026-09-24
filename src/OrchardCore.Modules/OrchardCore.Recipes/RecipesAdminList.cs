namespace OrchardCore.Recipes;

/// <summary>
/// The admin list of recipes rendered by the <c>AdminList</c> shape. The page renders one list per feature,
/// all sharing this name so a column provider configures them together.
/// Its columns are declared by <see cref="RecipesAdminListColumnProvider"/>.
/// </summary>
public static class RecipesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Recipes</c> and <c>AdminListCell__Recipes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Recipes";
}
