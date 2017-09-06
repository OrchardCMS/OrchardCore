using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRecipes(this IServiceCollection services)
        {
            services.AddScoped<IRecipeHarvester, ApplicationRecipeHarvester>();
            services.AddScoped<IRecipeHarvester, RecipeHarvester>();
            services.AddScoped<IRecipeExecutor, RecipeExecutor>();

            return services;
        }
    }
}
