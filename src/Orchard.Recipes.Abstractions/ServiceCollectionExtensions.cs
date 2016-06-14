using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orchard.Recipes.Services;

namespace Orchard.Recipes
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRecipeOptions(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RecipeHarvestingOptions>, RecipeHarvestingOptionsSetup>());
        }
    }
}