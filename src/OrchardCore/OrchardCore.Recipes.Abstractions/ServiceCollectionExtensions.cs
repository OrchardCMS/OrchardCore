using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRecipeExecutionStep<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IRecipeStepHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRecipeStepHandler, TImplementation>());

        return services;
    }

    /// <summary>
    /// Registers an optional validator of a recipe step. It is registered in the service container, as it is used
    /// across modules and generally depends on the services of the feature that handles the step.
    /// </summary>
    public static IServiceCollection AddRecipeStepValidator<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IRecipeStepValidator
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IRecipeStepValidator, TImplementation>());

        return services;
    }
}
