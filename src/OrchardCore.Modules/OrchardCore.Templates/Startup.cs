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

namespace OrchardCore.Templates
{
    public class Startup : StartupBase
    {
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
        }
    }
}
