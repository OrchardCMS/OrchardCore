using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a recipe step handler.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the recipe step handler.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRecipeExecutionStep<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IRecipeStepHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRecipeStepHandler, TImplementation>());

        return services;
    }

    /// <summary>
    /// Registers a recipe step descriptor that provides metadata and schema information about a recipe step.
    /// </summary>
    /// <typeparam name="TDescriptor">The type of the recipe step descriptor.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRecipeStepDescriptor<TDescriptor>(this IServiceCollection services)
        where TDescriptor : class, IRecipeStepDescriptor
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IRecipeStepDescriptor, TDescriptor>());

        return services;
    }

    /// <summary>
    /// Registers a recipe step schema provider that supplies JSON schema for a specific step.
    /// </summary>
    /// <typeparam name="TProvider">The type of the schema provider.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRecipeStepSchemaProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IRecipeStepSchemaProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IRecipeStepSchemaProvider, TProvider>());

        return services;
    }
}
