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

        // Register the ServiceProvider-based recipe harvester for code-defined recipes.
        // This harvester collects IRecipeDescriptor instances registered via AddRecipe<T>().
        services.AddScoped<IRecipeHarvester, ServiceProviderRecipeHarvester>();

        services.AddTransient<IRecipeExecutor, RecipeExecutor>();
        services.AddScoped<IRecipeMigrator, RecipeMigrator>();
        services.AddScoped<IRecipeReader, RecipeReader>();
        services.AddScoped<IRecipeEnvironmentProvider, RecipeEnvironmentFeatureProvider>();

        // Register the recipe schema services for schema discovery and validation.
        services.TryAddScoped<IRecipeSchemaService, RecipeSchemaService>();
        services.TryAddSingleton<IRecipeStepSchemaEvaluator, RecipeStepSchemaEvaluator>();

        // Register the unified recipe/deployment step handler bridge.
        // This allows IRecipeDeploymentStep implementations to work with the existing pipeline
        // during migration from the obsolete IRecipeStepHandler interface.
#pragma warning disable CS0618 // Type or member is obsolete - required for backwards compatibility
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRecipeStepHandler, RecipeDeploymentStepHandler>());
#pragma warning restore CS0618

        return services;
    }
}
