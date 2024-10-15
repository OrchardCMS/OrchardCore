using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Templates.Deployment;
using OrchardCore.Templates.Recipes;
using OrchardCore.Templates.Services;
using OrchardCore.Templates.Settings;

namespace OrchardCore.Templates;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();

        services.AddScoped<IShapeBindingResolver, TemplatesShapeBindingResolver>();
        services.AddScoped<PreviewTemplatesProvider>();
        services.AddScoped<TemplatesManager>();
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddRecipeExecutionStep<TemplateStep>();

        // Template shortcuts in settings
        services.AddScoped<IContentPartDefinitionDisplayDriver, TemplateContentPartDefinitionDriver>();
        services.AddScoped<IContentTypeDefinitionDisplayDriver, TemplateContentTypeDefinitionDriver>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, TemplateContentTypePartDefinitionDriver>();

        services.AddDeployment<AllTemplatesDeploymentSource, AllTemplatesDeploymentStep, AllTemplatesDeploymentStepDriver>();

        services.AddScoped<AdminTemplatesManager>();
        services.AddPermissionProvider<AdminTemplatesPermissions>();
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
    }
}
