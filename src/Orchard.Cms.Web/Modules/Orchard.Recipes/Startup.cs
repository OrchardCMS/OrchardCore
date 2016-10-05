using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;
using Orchard.Recipes.Models;
using Orchard.Recipes.Providers.Executors;
using Orchard.Recipes.Providers.RecipeHandlers;
using Orchard.Recipes.Services;
using YesSql.Core.Indexes;

namespace Orchard.Recipes
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRecipeHarvester, RecipeHarvester>();

            services.AddScoped<IRecipeExecutor, RecipeExecutor>();
            {
                services.AddScoped<IRecipeParser, JsonRecipeParser>();
                services.AddScoped<IRecipeStepExecutor, RecipeStepExecutor>();
            }

            services.AddScoped<IRecipeResultAccessor, RecipeResultAccessor>();
            services.AddScoped<IIndexProvider, RecipeResultIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IRecipeHandler, RecipeExecutionStepHandler>();

            services.AddRecipeExecutionStep<CommandStep>();
            services.AddRecipeExecutionStep<RecipesStep>();

            services.AddRecipeOptions();
            services.AddRecipeExtension<JsonRecipeParser>("*.recipe.json");
        }
    }
}
