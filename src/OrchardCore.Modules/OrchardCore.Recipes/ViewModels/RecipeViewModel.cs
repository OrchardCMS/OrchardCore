namespace OrchardCore.Recipes.ViewModels;

public class RecipeViewModel
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string[] Tags { get; set; }

    public bool IsSetupRecipe { get; set; }

    public string Description { get; set; }
}
