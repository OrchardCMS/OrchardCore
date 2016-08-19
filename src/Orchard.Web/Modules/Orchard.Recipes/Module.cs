using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Commands;
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
            services.AddScoped<IRecipeStepQueue, RecipeStepQueue>();
            services.AddScoped<IRecipeScheduler, RecipeScheduler>();
            services.AddScoped<IRecipeManager, RecipeManager>();
            services.AddScoped<IRecipeStepExecutor, RecipeStepExecutor>();

            services.AddScoped<IRecipeHandler, RecipeExecutionStepHandler>();

            services.AddScoped<IRecipeExecutionStep, ActivateShellStep>();
            services.AddScoped<IRecipeExecutionStep, CommandStep>().AddCommands(); // Should I do .AddCommands() here? (ngm)

            services.AddRecipeOptions();
            services.AddRecipeExtension("*.recipe.json", typeof(JsonRecipeParser));
        }
    }
}
