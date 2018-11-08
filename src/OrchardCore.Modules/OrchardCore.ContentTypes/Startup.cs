using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentTypes.Deployment;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.RecipeSteps;
using OrchardCore.ContentTypes.Services;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTypes
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IContentDefinitionService, ContentDefinitionService>();
            services.AddScoped<IStereotypesProvider, DefaultStereotypesProvider>();
            services.AddScoped<IStereotypeService, StereotypeService>();
            services.AddScoped<IContentDefinitionDisplayHandler, ContentDefinitionDisplayCoordinator>();
            services.AddScoped<IContentDefinitionDisplayManager, DefaultContentDefinitionDisplayManager>();
            services.AddScoped<IContentPartDefinitionDisplayDriver, ContentPartSettingsDisplayDriver>();
            services.AddScoped<IContentTypeDefinitionDisplayDriver, ContentTypeSettingsDisplayDriver>();
            services.AddScoped<IContentTypeDefinitionDisplayDriver, DefaultContentTypeDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartSettingsDisplayDriver>();

            // TODO: Put in its own feature to be able to execute this recipe without having to enable
            // Content Types management UI
            services.AddRecipeExecutionStep<ContentDefinitionStep>();

        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "EditField",
                areaName: "OrchardCore.ContentTypes",
                template: "Admin/ContentParts/{id}/Fields/{name}/Edit",
                defaults: new { controller = "Admin", action = "EditField" }
            );

            routes.MapAreaRoute(
                name: "EditTypePart",
                areaName: "OrchardCore.ContentTypes",
                template: "Admin/ContentTypes/{id}/ContentParts/{name}/Edit",
                defaults: new { controller = "Admin", action = "EditTypePart" }
            );

            routes.MapAreaRoute(
                name: "RemovePart",
                areaName: "OrchardCore.ContentTypes",
                template: "Admin/ContentTypes/{id}/ContentParts/{name}/Remove",
                defaults: new { controller = "Admin", action = "RemovePart" }
            );
        }
    }


    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, ContentDefinitionDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ContentDefinitionDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, ContentDefinitionDeploymentStepDriver>();
        }
    }
}
