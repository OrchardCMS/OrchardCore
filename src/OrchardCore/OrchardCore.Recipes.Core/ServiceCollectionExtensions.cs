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
            services.AddTransient<IRecipeExecutor, RecipeExecutor>();
            services.AddScoped<IRecipeMigrator, RecipeMigrator>();
            services.AddScoped<IRecipeReader, RecipeReader>();
            services.AddScoped<IRecipeEnvironmentProvider, RecipeEnvironmentFeatureProvider>();

            return services;
        }
    }
}
