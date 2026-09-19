using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Templates.Deployment;
using OrchardCore.Templates.Recipes;
using OrchardCore.Templates.Services;
using OrchardCore.Templates.ViewModels;
using OrchardCore.Templates.Drivers;
using OrchardCore.DisplayManagement.Handlers;
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

        // Builds the rows of the templates admin list, shared by the site and admin template lists.
        services.AddDisplayDriver<TemplateEntry, TemplateEntryDisplayDriver>();
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
