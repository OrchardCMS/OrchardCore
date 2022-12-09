using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentTypes.Controllers;
using OrchardCore.ContentTypes.Deployment;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.RecipeSteps;
using OrchardCore.ContentTypes.Services;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Events;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTypes
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

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
            services.AddRecipeExecutionStep<ReplaceContentDefinitionStep>();
            services.AddRecipeExecutionStep<DeleteContentDefinitionStep>();

            services.AddTransient<IRecipeEventHandler, LuceneRecipeEventHandler>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "EditField",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentParts/{id}/Fields/{name}/Edit",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.EditField) }
            );

            routes.MapAreaControllerRoute(
                name: "CreateType",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentTypes/Create",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "AddFieldsTo",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentTypes/AddFieldsTo/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.AddFieldTo) }
            );

            routes.MapAreaControllerRoute(
                name: "AddPartsTo",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentTypes/AddPartsTo/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.AddPartsTo) }
            );

            routes.MapAreaControllerRoute(
                name: "AddReusablePartTo",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentTypes/AddReusablePartTo/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.AddReusablePartTo) }
            );

            routes.MapAreaControllerRoute(
                name: "EditType",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentTypes/Edit/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "EditTypePart",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentTypes/{id}/ContentParts/{name}/Edit",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.EditTypePart) }
            );

            routes.MapAreaControllerRoute(
                name: "EditPart",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentParts/Edit/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.EditPart) }
            );

            routes.MapAreaControllerRoute(
                name: "ListContentTypes",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentTypes/List",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.List) }
            );

            routes.MapAreaControllerRoute(
                name: "ListContentParts",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentTypes/ListParts",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.ListParts) }
            );

            routes.MapAreaControllerRoute(
                name: "RemovePart",
                areaName: "OrchardCore.ContentTypes",
                pattern: _adminOptions.AdminUrlPrefix + "/ContentTypes/{id}/ContentParts/{name}/Remove",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.RemovePart) }
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

            services.AddTransient<IDeploymentSource, ReplaceContentDefinitionDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ReplaceContentDefinitionDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, ReplaceContentDefinitionDeploymentStepDriver>();

            services.AddTransient<IDeploymentSource, DeleteContentDefinitionDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<DeleteContentDefinitionDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, DeleteContentDefinitionDeploymentStepDriver>();
        }
    }
}
