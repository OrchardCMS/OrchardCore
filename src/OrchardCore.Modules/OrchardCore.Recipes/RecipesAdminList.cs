namespace OrchardCore.Recipes;

/// <summary>
/// The admin list of recipes rendered by the <c>AdminList</c> shape. The page renders one list per feature,
/// all sharing this name so the same column providers declare their columns.
/// Its columns are declared by the rendering view.
/// </summary>
public static class RecipesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column providers and used by the
    /// <c>AdminList__Recipes</c> and <c>AdminListCell__Recipes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Recipes";
}
