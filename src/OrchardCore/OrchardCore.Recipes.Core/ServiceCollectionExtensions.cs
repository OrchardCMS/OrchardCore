using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes;

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

        // Register the recipe schema service for schema discovery and validation.
        services.TryAddSingleton<IRecipeSchemaService, RecipeSchemaService>();

        return services;
    }
}
