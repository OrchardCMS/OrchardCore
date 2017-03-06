using Microsoft.Extensions.DependencyInjection;
using Orchard.Recipes.Services;

namespace Orchard.Recipes
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRecipes(this IServiceCollection services)
        {
            services.AddScoped<IRecipeHarvester, RecipeHarvester>();
            services.AddScoped<IRecipeExecutor, RecipeExecutor>();

            return services;
        }
    }
}
