namespace OrchardCore.Recipes;

/// <summary>
/// The names of the breadcrumbs rendered by the recipes screens. A module adds a node to one of these trails by
/// registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class RecipesBreadcrumbs
{
    /// <summary>
    /// The breadcrumb of the recipes list. It is named after the list of that screen.
    /// </summary>
    public const string List = "Recipes";
}
