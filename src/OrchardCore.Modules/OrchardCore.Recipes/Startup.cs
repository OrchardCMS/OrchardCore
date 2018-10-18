using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.Navigation;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.RecipeSteps;
using OrchardCore.Recipes.Services;
using YesSql.Indexes;

namespace OrchardCore.Recipes
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
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<IIndexProvider, RecipeResultIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();

            services.AddRecipeExecutionStep<CommandStep>();
            services.AddRecipeExecutionStep<RecipesStep>();

            services.AddDeploymentTargetHandler<RecipeDeploymentTargetHandler>();
        }
    }
}
