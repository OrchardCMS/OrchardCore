using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public interface IRecipeReader
    {
        Task<RecipeDescriptor> GetRecipeDescriptor(string recipeFilePath, IFileProvider fileProvider);

        Task<RecipeDescriptor> GetRecipeDescriptor(string recipeFilePath, IFileInfo recipeFileInfo, IFileProvider fileProvider);
    }
}
