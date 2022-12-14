using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRecipes(this IServiceCollection services)
        {
            services.TryAddScoped<IRecipeHarvester, ApplicationRecipeHarvester>();
            services.TryAddScoped<IRecipeHarvester, RecipeHarvester>();
            services.TryAddTransient<IRecipeExecutor, RecipeExecutor>();
            services.TryAddScoped<IRecipeMigrator, RecipeMigrator>();
            services.TryAddScoped<IRecipeReader, RecipeReader>();
            services.TryAddScoped<IRecipeEnvironmentProvider, RecipeEnvironmentFeatureProvider>();

            return services;
        }
    }
}
