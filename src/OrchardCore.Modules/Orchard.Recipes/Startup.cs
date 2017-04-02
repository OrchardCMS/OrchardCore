using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data.Migration;
using Orchard.Recipes.Models;
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

            services.AddRecipeExecutionStep<CommandStep>();
            services.AddRecipeExecutionStep<RecipesStep>();
        }
    }
}
