using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes.Controllers;
using OrchardCore.Recipes.RecipeSteps;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddRecipeExecutionStep<CommandStep>();
            services.AddRecipeExecutionStep<RecipesStep>();

            services.AddDeploymentTargetHandler<RecipeDeploymentTargetHandler>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Recipes",
                areaName: "OrchardCore.Recipes",
                pattern: _adminOptions.AdminUrlPrefix + "/Recipes",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "RecipesExecute",
                areaName: "OrchardCore.Recipes",
                pattern: _adminOptions.AdminUrlPrefix + "/Recipes/Execute",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Execute) }
            );
        }
    }

    [Feature("OrchardCore.Recipes.Core")]
    public class RecipesCoreStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddRecipes();
        }
    }
}
