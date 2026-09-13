using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.RemoteManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Templates.Deployment;
using OrchardCore.Templates.Endpoints.Management;
using OrchardCore.Templates.Recipes;
using OrchardCore.Templates.Services;
using OrchardCore.Templates.Settings;

namespace OrchardCore.Templates;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

        services.AddScoped<IShapeBindingResolver, TemplatesShapeBindingResolver>();
        services.AddScoped<PreviewTemplatesProvider>();
        services.AddScoped<TemplatesManager>();
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddRecipeExecutionStep<TemplateStep>();
        services.AddSingleton<IRemoteManagementCapabilityProvider, TemplatesRemoteManagementCapabilityProvider>();

        // Template shortcuts in settings
        services.AddScoped<IContentPartDefinitionDisplayDriver, TemplateContentPartDefinitionDriver>();
        services.AddScoped<IContentTypeDefinitionDisplayDriver, TemplateContentTypeDefinitionDriver>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, TemplateContentTypePartDefinitionDriver>();

        services.AddDeployment<AllTemplatesDeploymentSource, AllTemplatesDeploymentStep, AllTemplatesDeploymentStepDriver>();
        services.AddSingleton<IDeploymentStepDefinition>(new BooleanDeploymentStepDefinition<AllTemplatesDeploymentStep>(
            nameof(AllTemplatesDeploymentStep), "exportAsFiles", step => step.ExportAsFiles, (step, value) => step.ExportAsFiles = value));

        services.AddScoped<AdminTemplatesManager>();
        services.AddPermissionProvider<AdminTemplatesPermissions>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddTemplateManagementEndpoints();
    }
}

[Feature("OrchardCore.AdminTemplates")]
public sealed class AdminTemplatesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IShapeBindingResolver, AdminTemplatesShapeBindingResolver>();
        services.AddScoped<AdminPreviewTemplatesProvider>();
        services.AddNavigationProvider<AdminTemplatesAdminMenu>();
        services.AddRecipeExecutionStep<AdminTemplateStep>();
        services.AddDeployment<AllAdminTemplatesDeploymentSource, AllAdminTemplatesDeploymentStep, AllAdminTemplatesDeploymentStepDriver>();
        services.AddSingleton<IDeploymentStepDefinition>(new BooleanDeploymentStepDefinition<AllAdminTemplatesDeploymentStep>(
            nameof(AllAdminTemplatesDeploymentStep), "exportAsFiles", step => step.ExportAsFiles, (step, value) => step.ExportAsFiles = value));
    }
}
