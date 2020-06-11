using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRecipeExecutionStep<TImplementation>(
            this IServiceCollection serviceCollection)
            where TImplementation : class, IRecipeStepHandler
        {
            serviceCollection.AddScoped<IRecipeStepHandler, TImplementation>();

            return serviceCollection;
        }
    }
}
