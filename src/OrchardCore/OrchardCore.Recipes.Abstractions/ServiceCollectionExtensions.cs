using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Modules;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a recipe step handler.
    /// </summary>
    /// <remarks>
    /// A handler declaring a <see cref="RecipeStepAttribute"/> is only created to execute the steps of that name.
    /// Any other <see cref="IRecipeStepHandler"/> is created for every step and filters the steps itself.
    /// </remarks>
    public static IServiceCollection AddRecipeExecutionStep<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IRecipeStepHandler
    {
        if (TypeMetadata.TryGet<RecipeStepAttribute>(typeof(TImplementation), out var metadata))
        {
            services.TryAddScoped<TImplementation>();
            services.Configure<RecipeStepHandlerOptions>(options => options.AddHandler(metadata.Name, typeof(TImplementation)));
        }
        else
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IRecipeStepHandler, TImplementation>());
        }

        return services;
    }
}
