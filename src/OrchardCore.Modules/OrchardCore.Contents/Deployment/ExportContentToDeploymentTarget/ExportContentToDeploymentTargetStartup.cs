using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

[Feature("OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget")]
public sealed class ExportContentToDeploymentTargetStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<ExportContentToDeploymentTargetAdminMenu>();

        services.AddSiteDisplayDriver<ExportContentToDeploymentTargetSettingsDisplayDriver>();

        services.AddDeployment<ExportContentToDeploymentTargetDeploymentSource, ExportContentToDeploymentTargetDeploymentStep, ExportContentToDeploymentTargetDeploymentStepDriver>();

        services.AddDataMigration<ExportContentToDeploymentTargetMigrations>();
        services.AddScoped<IContentDisplayDriver, ExportContentToDeploymentTargetContentDriver>();
        services.AddDisplayDriver<ContentOptionsViewModel, ExportContentToDeploymentTargetContentsAdminListDisplayDriver>();

        services.AddSiteSettingsPropertyDeploymentStep<ExportContentToDeploymentTargetSettings, ExportContentToDeploymentTargetStartup>(S => S["Export Content To Deployment Target settings"], S => S["Exports the Export Content To Deployment Target settings."]);
    }
}
