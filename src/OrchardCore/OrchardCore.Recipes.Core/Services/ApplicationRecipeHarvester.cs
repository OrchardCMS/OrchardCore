using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Finds recipes in the application content folder.
/// </summary>
public class ApplicationRecipeHarvester : RecipeHarvester
{
    public ApplicationRecipeHarvester(
        IRecipeReader recipeReader,
        IExtensionManager extensionManager,
        IHostEnvironment hostingEnvironment,
        ILogger<RecipeHarvester> logger)
        : base(recipeReader, extensionManager, hostingEnvironment, logger)
    {
    }

    public override Task<IEnumerable<IRecipeDescriptor>> HarvestRecipesAsync()
    {
        return HarvestRecipesAsync("Recipes");
    }
}
