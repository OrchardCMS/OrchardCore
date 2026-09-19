using OrchardCore.DisplayManagement;

namespace OrchardCore.Recipes.ViewModels;

public class RecipeViewModel
{
    public string FileName { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string[] Tags { get; set; }
    public bool IsSetupRecipe { get; set; }
    public string Description { get; set; }
    public string BasePath { get; set; }
    public string Feature { get; set; }
}


/// <summary>
/// The recipes admin list, one group per feature. Each group carries its own <c>AdminList</c> shape so the
/// page keeps the grouping it had while every group follows the configured layout.
/// </summary>
public class RecipesIndexViewModel
{
    public IList<RecipeGroupViewModel> Groups { get; set; } = [];

    /// <summary>
    /// The layouts a user can switch the list to, or <see langword="null"/> when the site keeps the choice to
    /// itself. The page carries it beside its search bar: its features share one layout, so a selector on each
    /// of them would offer the same thing over and over.
    /// </summary>
    public IShape LayoutSelector { get; set; }
}

public class RecipeGroupViewModel
{
    public string Feature { get; set; }

    /// <summary>
    /// The text the client-side search matches the whole group against.
    /// </summary>
    public string FilterValue { get; set; }

    public dynamic List { get; set; }
}
