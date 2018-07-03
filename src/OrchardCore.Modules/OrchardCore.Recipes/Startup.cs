using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.RecipeSteps;
using OrchardCore.Recipes.Services;
using YesSql.Indexes;
using OrchardCore.Environment.Navigation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

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
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "ExecuteRecipe",
                areaName: "OrchardCore.Recipes",
                template: "Admin/Recipes/Execute/{path}",
                defaults: new { controller = "Admin", action = "Execute" }
            );
        }
    }
}
