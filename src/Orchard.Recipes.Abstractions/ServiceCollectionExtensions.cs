using Microsoft.Extensions.DependencyInjection;
using Orchard.Recipes.Services;

namespace Orchard.Recipes
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRecipeExecutionStep<TImplementation>(
            this IServiceCollection serviceCollection)
            where TImplementation : class, IRecipeExecutionStep
        {
            serviceCollection.AddScoped<IRecipeExecutionStep, TImplementation>();

            return serviceCollection;
        }
    }
}