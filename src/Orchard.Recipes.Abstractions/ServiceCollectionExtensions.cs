using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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

        public static void AddRecipeOptions(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RecipeHarvestingOptions>, RecipeHarvestingOptionsSetup>());
        }

        public static IServiceCollection AddRecipeExtension<TParser>(
            this IServiceCollection services,
            string fileExtension)
            where TParser : class, IRecipeParser
        {
            return services.Configure<RecipeHarvestingOptions>(configureOptions: options =>
            {
                options.RecipeFileExtensions.Add(fileExtension, typeof(TParser));
            });
        }
    }
}