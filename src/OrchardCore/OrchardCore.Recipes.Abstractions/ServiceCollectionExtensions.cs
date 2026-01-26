using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a unified recipe/deployment step that handles both import and export
    /// using JSON Schema as the single source of truth.
    /// </summary>
    /// <typeparam name="TStep">The type of the unified step.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRecipeDeploymentStep<TStep>(this IServiceCollection services)
        where TStep : class, IRecipeDeploymentStep
    {
        // Register as the unified interface.
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRecipeDeploymentStep, TStep>());

        return services;
    }

    /// <summary>
    /// Registers a code-based recipe descriptor that can be discovered by the ServiceProviderRecipeHarvester.
    /// This allows recipes to be defined entirely in code.
    /// </summary>
    /// <typeparam name="TRecipe">The type of the recipe descriptor.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRecipe<TRecipe>(this IServiceCollection services)
        where TRecipe : class, IRecipeDescriptor
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRecipeDescriptor, TRecipe>());

        return services;
    }

    /// <summary>
    /// Registers a recipe step handler.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the recipe step handler.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method is obsolete. Use <see cref="AddRecipeDeploymentStep{TStep}"/> instead.
    /// </remarks>
    [Obsolete($"Use {nameof(AddRecipeDeploymentStep)} instead. This method will be removed in a future version.", false)]
    public static IServiceCollection AddRecipeExecutionStep<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IRecipeStepHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRecipeStepHandler, TImplementation>());

        return services;
    }
}
