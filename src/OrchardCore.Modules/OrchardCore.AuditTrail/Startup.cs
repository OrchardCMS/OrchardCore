using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AuditTrail.Controllers;
using OrchardCore.AuditTrail.Drivers;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Navigation;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;
using YesSql.Indexes;

namespace OrchardCore.AuditTrail
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
            services.AddScoped<IAuditTrailManager, AuditTrailManager>();
            services.AddScoped<IAuditTrailEventHandler, AuditTrailEventHandler>();

            // This has to go
            services.AddScoped<IAuditTrailDisplayManager, AuditTrailtDisplayManager>();
            services.AddScoped<IAuditTrailDisplayHandler, AuditTrailDisplayHandler>();

            // and be replaced by this
            services
                .AddScoped<IDisplayManager<AuditTrailEvent>, DisplayManager<AuditTrailEvent>>()
                .AddScoped<IDisplayDriver<AuditTrailEvent>, AuditTrailEventDisplayDriver>();


            services.AddSingleton<IAuditTrailIdGenerator, AuditTrailIdGenerator>();

            services.Configure<StoreCollectionOptions>(o => o.Collections.Add(AuditTrailEvent.Collection));

            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, AuditTrailEventIndexProvider>();
            services.AddSingleton<IBackgroundTask, AuditTrailBackgroundTask>();

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AuditTrailAdminMenu>();
            services.AddScoped<INavigationProvider, AuditTrailSettingsAdminMenu>();

            services.AddScoped<IDisplayDriver<ISite>, AuditTrailSettingsDisplayDriver>();
            services.AddScoped<IDisplayDriver<ISite>, AuditTrailTrimmingSettingsDisplayDriver>();

            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<AuditTrailSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<Startup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<AuditTrailSettings>(S["Audit Trail settings"], S["Exports the audit trail settings."]);
            });

            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<AuditTrailSettings>());

            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<AuditTrailTrimmingSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<Startup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<AuditTrailTrimmingSettings>(S["Audit Trail Trimming settings"], S["Exports the audit trail trimming settings."]);
            });

            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<AuditTrailTrimmingSettings>());
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "AuditTrailIndex",
                areaName: "OrchardCore.AuditTrail",
                pattern: _adminOptions.AdminUrlPrefix + "/AuditTrail",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "AuditTrailDisplay",
                areaName: "OrchardCore.AuditTrail",
                pattern: _adminOptions.AdminUrlPrefix + "/AuditTrail/Display/{auditTrailEventId}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Display) }
            );
        }
    }
}
