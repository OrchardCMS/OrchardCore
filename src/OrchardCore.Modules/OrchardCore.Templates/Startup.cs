using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Templates.Deployment;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Recipes;
using OrchardCore.Templates.Services;
using OrchardCore.Templates.Settings;

namespace OrchardCore.Templates;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

        // 'TemplatesDocument' is stored in the shared document table and can be loaded by id during
        // an early request (e.g. the login page), so its type is pre-registered with the store to
        // avoid a 'KeyNotFoundException' when YesSql resolves the row's persisted type name.
        services.AddDocumentType<TemplatesDocument>();

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
        services.AddDocumentType<AdminTemplatesDocument>();

        services.AddScoped<IShapeBindingResolver, AdminTemplatesShapeBindingResolver>();
        services.AddScoped<AdminPreviewTemplatesProvider>();
        services.AddNavigationProvider<AdminTemplatesAdminMenu>();
        services.AddRecipeExecutionStep<AdminTemplateStep>();
        services.AddDeployment<AllAdminTemplatesDeploymentSource, AllAdminTemplatesDeploymentStep, AllAdminTemplatesDeploymentStepDriver>();
    }
}
