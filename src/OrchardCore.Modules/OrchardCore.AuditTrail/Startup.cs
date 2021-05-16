using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Drivers;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Navigation;
using OrchardCore.AuditTrail.Permissions;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;
using YesSql.Indexes;

namespace OrchardCore.AuditTrail
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IAuditTrailManager, AuditTrailManager>();
            services.AddScoped<IAuditTrailEventHandler, AuditTrailEventHandler>();
            services.AddScoped<IAuditTrailDisplayManager, AuditTrailtDisplayManager>();
            services.AddScoped<IAuditTrailDisplayHandler, AuditTrailDisplayHandler>();
            services.AddSingleton<IAuditTrailIdGenerator, AuditTrailIdGenerator>();

            services.Configure<StoreCollectionOptions>(o => o.Collections.Add(AuditTrailEvent.Collection));

            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, AuditTrailEventIndexProvider>();
            services.AddSingleton<IBackgroundTask, AuditTrailBackgroundTask>();

            services.AddScoped<IPermissionProvider, AuditTrailPermissions>();
            services.AddScoped<INavigationProvider, AuditTrailAdminMenu>();
            services.AddScoped<INavigationProvider, AuditTrailSettingsAdminMenu>();

            services.AddContentPart<AuditTrailPart>()
                .UseDisplayDriver<AuditTrailPartDisplayDriver>();

            services.AddScoped<IDisplayDriver<ISite>, AuditTrailSettingsDisplayDriver>();
            services.AddScoped<IDisplayDriver<ISite>, AuditTrailTrimmingSettingsDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AuditTrailPartSettingsDisplayDriver>();

            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<AuditTrailSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var T = sp.GetService<IStringLocalizer<Startup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<AuditTrailSettings>(T["Audit Trail settings"], T["Exports the audit trail settings."]);
            });

            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<AuditTrailSettings>());

            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<AuditTrailTrimmingSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var T = sp.GetService<IStringLocalizer<Startup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<AuditTrailTrimmingSettings>(T["Audit Trail Trimming settings"], T["Exports the audit trail trimming settings."]);
            });

            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<AuditTrailTrimmingSettings>());
        }
    }
}
