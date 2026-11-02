namespace OrchardCore.Recipes.Models;

/// <summary>
/// One row of the recipes admin list. Its name is the shape type of the rows built by
/// <see cref="Drivers.RecipeEntryDisplayDriver"/>, so themes override them with
/// <c>RecipeEntry-SummaryAdmin.cshtml</c>.
/// </summary>
public class RecipeEntry
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
