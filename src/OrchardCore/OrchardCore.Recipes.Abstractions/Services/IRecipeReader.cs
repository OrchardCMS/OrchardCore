using System;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public interface IRecipeReader
    {
        [Obsolete("This method has been deprecated, please use GetRecipeDescriptorAsync(string, IFileInfo, IFileProvider).")]
        async Task<RecipeDescriptor> GetRecipeDescriptor(string recipeBasePath, IFileInfo recipeFileInfo, IFileProvider fileProvider)
            => await GetRecipeDescriptorAsync(recipeBasePath, recipeFileInfo, fileProvider);

        Task<RecipeDescriptor> GetRecipeDescriptorAsync(string recipeBasePath, IFileInfo recipeFileInfo, IFileProvider fileProvider);
    }
}
