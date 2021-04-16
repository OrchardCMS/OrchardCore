using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget
{
    [Feature("OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget")]
    public class ExportContentToDeploymentTargetStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, ExportContentToDeploymentTargetAdminMenu>();
            // TODO deployment steps for these
            services.AddTransient<IDisplayDriver<ISite>, ExportContentToDeploymentTargetSettingsDisplayDriver>();

            services.AddTransient<IDeploymentSource, ExportContentToDeploymentTargetDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ExportContentToDeploymentTargetDeploymentStep>());
            services.AddTransient<IDisplayDriver<DeploymentStep>, ExportContentToDeploymentTargetDeploymentStepDriver>();

            services.AddTransient<IDataMigration, ExportContentToDeploymentTargetMigrations>();
            services.AddTransient<IContentDisplayDriver, ExportContentToDeploymentTargetContentDriver>();
            services.AddTransient<IDisplayDriver<ContentOptionsViewModel>, ExportContentToDeploymentTargetContentsAdminListDisplayDriver>();

            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<ExportContentToDeploymentTargetSettings>>();
            services.AddTransient<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<ExportContentToDeploymentTargetStartup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<ExportContentToDeploymentTargetSettings>(S["Export Content To Deployment Target settings"], S["Exports the Export Content To Deployment Target settings."]);
            });
            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<ExportContentToDeploymentTargetSettings>());

        }
    }
}
