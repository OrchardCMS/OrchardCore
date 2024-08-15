using Microsoft.Extensions.FileProviders;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

public interface IRecipeReader
{
    Task<RecipeDescriptor> GetRecipeDescriptorAsync(string recipeBasePath, IFileInfo recipeFileInfo, IFileProvider fileProvider);
}
