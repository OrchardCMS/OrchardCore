using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Recipes.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRecipeExtension(
            this IServiceCollection services,
            string fileExtension)
        {
            return services.Configure<RecipeHarvestingOptions>(configureOptions: options =>
            {
                options.RecipeFileExtensions.Add(fileExtension);
            });
        }
    }
}