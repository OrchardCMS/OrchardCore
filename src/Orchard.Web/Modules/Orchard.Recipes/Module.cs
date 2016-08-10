using Microsoft.Extensions.DependencyInjection;
using Orchard.Recipes.Services;

namespace Orchard.Recipes
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Module : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRecipeParser, JsonRecipeParser>();
            services.AddScoped<IRecipeHarvester, RecipeHarvester>();
            services.AddScoped<IRecipeStepQueue, RecipeStepQueue>();
            services.AddScoped<IRecipeScheduler, RecipeScheduler>();
            services.AddScoped<IRecipeManager, RecipeManager>();
            services.AddScoped<IRecipeStepExecutor, RecipeStepExecutor>();

            services.AddRecipeOptions();
            services.AddRecipeExtension("*.recipe.json", typeof(JsonRecipeParser));
        }
    }
}
