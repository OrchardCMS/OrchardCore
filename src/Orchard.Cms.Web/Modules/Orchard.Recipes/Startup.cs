using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;
using Orchard.Recipes.Models;
using Orchard.Recipes.RecipeHandlers;
using Orchard.Recipes.RecipeSteps;
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
            services.AddRecipes();

            services.AddScoped<IRecipeStore, RecipeStore>();

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
