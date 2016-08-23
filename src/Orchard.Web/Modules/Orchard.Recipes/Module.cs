using Microsoft.Extensions.DependencyInjection;
using Orchard.Recipes.Providers.Executors;
using Orchard.Recipes.Providers.RecipeHandlers;
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

            services.AddScoped<IRecipeManager, RecipeManager>();
            {
                services.AddScoped<IRecipeExecutor, RecipeExecutor>();
                {
                    services.AddScoped<IRecipeParser, JsonRecipeParser>();
                    services.AddScoped<IRecipeStepExecutor, RecipeStepExecutor>();
                }
            }

            services.AddScoped<IRecipeResultAccessor, RecipeResultAccessor>();

            services.AddScoped<IRecipeHandler, RecipeExecutionStepHandler>();

            services.AddRecipeExecutionStep<CommandStep>();
            services.AddRecipeExecutionStep<RecipesStep>();

            services.AddRecipeOptions();
            services.AddRecipeExtension<JsonRecipeParser>("*.recipe.json");
        }
    }
}
