using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Modules;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Templates.Deployment;
using OrchardCore.Templates.Recipes;
using OrchardCore.Templates.Services;
using OrchardCore.Templates.Settings;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Templates.Controllers;
using System;

namespace OrchardCore.Templates
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
            services.AddScoped<IShapeBindingResolver, TemplatesShapeBindingResolver>();
            services.AddScoped<PreviewTemplatesProvider>();
            services.AddScoped<TemplatesManager>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddRecipeExecutionStep<TemplateStep>();

            // Template shortcuts in settings
            services.AddScoped<IContentPartDefinitionDisplayDriver, TemplateContentPartDefinitionDriver>();
            services.AddScoped<IContentTypeDefinitionDisplayDriver, TemplateContentTypeDefinitionDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, TemplateContentTypePartDefinitionDriver>();

            services.AddTransient<IDeploymentSource, AllTemplatesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllTemplatesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllTemplatesDeploymentStepDriver>();

            services.AddScoped<AdminTemplatesManager>();
            services.AddScoped<IPermissionProvider, AdminTemplatesPermissions>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Templates.Index",
                areaName: "OrchardCore.Templates",
                pattern: _adminOptions.AdminUrlPrefix + "/Templates",
                defaults: new { controller = typeof(TemplateController).ControllerName(), action = nameof(TemplateController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "Templates.Create",
                areaName: "OrchardCore.Templates",
                pattern: _adminOptions.AdminUrlPrefix + "/Templates/Create",
                defaults: new { controller = typeof(TemplateController).ControllerName(), action = nameof(TemplateController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "Templates.Edit",
                areaName: "OrchardCore.Templates",
                pattern: _adminOptions.AdminUrlPrefix + "/Templates/Edit/{name}",
                defaults: new { controller = typeof(TemplateController).ControllerName(), action = nameof(TemplateController.Edit) }
            );
        }
    }
    
    [Feature("OrchardCore.AdminTemplates")]
    public class AdminTemplatesStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public AdminTemplatesStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShapeBindingResolver, AdminTemplatesShapeBindingResolver>();
            services.AddScoped<AdminPreviewTemplatesProvider>();
            services.AddScoped<INavigationProvider, AdminTemplatesAdminMenu>();
            services.AddRecipeExecutionStep<AdminTemplateStep>();

            services.AddTransient<IDeploymentSource, AllAdminTemplatesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllAdminTemplatesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllAdminTemplatesDeploymentStepDriver>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Templates.Admin",
                areaName: "OrchardCore.Templates",
                pattern: _adminOptions.AdminUrlPrefix + "/Templates/Admin",
                defaults: new { controller = typeof(TemplateController).ControllerName(), action = nameof(TemplateController.Admin) }
            );
        }
    }
}
