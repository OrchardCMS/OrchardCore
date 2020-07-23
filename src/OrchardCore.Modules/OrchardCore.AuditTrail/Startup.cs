using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Drivers;
using OrchardCore.AuditTrail.Handlers;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Migrations;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Navigation;
using OrchardCore.AuditTrail.Permissions;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.AuditTrail.Shapes;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using YesSql.Indexes;

namespace OrchardCore.AuditTrail
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<AuditTrailPart>()
                .UseDisplayDriver<AuditTrailPartDisplayDriver>();

            services.AddScoped<IDataMigration, AuditTrailMigrations>();

            services.AddSingleton<IIndexProvider, AuditTrailEventIndexProvider>();
            services.AddSingleton<IIndexProvider, ContentAuditTrailEventIndexProvider>();

            services.AddScoped<IPermissionProvider, AuditTrailPermissions>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, AuditTrailPartSettingsDisplayDriver>();

            services.AddScoped<IAuditTrailManager, AuditTrailManager>();

            services.AddScoped<IAuditTrailEventHandler, CommonAuditTrailEventHandler>();
            services.AddScoped<IAuditTrailEventHandler, ContentAuditTrailEventHandler>();

            services.AddScoped<INavigationProvider, AuditTrailSettingsAdminMenu>();
            services.AddScoped<INavigationProvider, AuditTrailAdminMenu>();

            services.AddScoped<IDisplayDriver<ISite>, AuditTrailSettingsDisplayDriver>();
            services.AddScoped<IDisplayDriver<ISite>, AuditTrailTrimmingSettingsDisplayDriver>();

            services.AddScoped<IAuditTrailEventDisplayManager, AuditTrailEventDisplayManager>();

            services.AddScoped<IShapeTableProvider, ContentAuditTrailEventShapesTableProvider>();

            services.AddScoped<IAuditTrailEventProvider, ContentAuditTrailEventProvider>();
            services.AddScoped<IAuditTrailEventProvider, UserAuditTrailEventProvider>();
            
            services.AddScoped<IAuditTrailContentEventHandler, AuditTrailContentTypesEvents>();

            services.AddSingleton<IBackgroundTask, AuditTrailTrimmingBackgroundTask>();
            
            services.AddScoped<IContentHandler, GlobalContentHandler>();
            services.AddScoped<IUserEventHandler, UserEventHandler>();
            services.AddScoped<ILoginFormEvent, UserEventHandler>();
            services.AddScoped<IPasswordRecoveryFormEvents, UserEventHandler>();
            services.AddScoped<IRegistrationFormEvents, UserEventHandler>();

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
