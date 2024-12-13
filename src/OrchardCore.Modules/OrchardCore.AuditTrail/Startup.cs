using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.AuditTrail.Drivers;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Navigation;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;
using YesSql.Filters.Query;

namespace OrchardCore.AuditTrail;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Add ILookupNormalizer as Singleton because it is needed by Users
        services.TryAddSingleton<ILookupNormalizer, UpperInvariantLookupNormalizer>();

        services.AddScoped<IAuditTrailManager, AuditTrailManager>();

        services.AddDisplayDriver<AuditTrailEvent, AuditTrailEventDisplayDriver>();

        services.AddSingleton<IAuditTrailIdGenerator, AuditTrailIdGenerator>();

        services.Configure<StoreCollectionOptions>(o => o.Collections.Add(AuditTrailEvent.Collection));

        services.AddDataMigration<Migrations>();
        services.AddIndexProvider<AuditTrailEventIndexProvider>();
        services.AddSingleton<IBackgroundTask, AuditTrailBackgroundTask>();

        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AuditTrailAdminMenu>();
        services.AddNavigationProvider<AuditTrailSettingsAdminMenu>();

        services.AddSiteDisplayDriver<AuditTrailSettingsDisplayDriver>();
        services.AddSiteDisplayDriver<AuditTrailTrimmingSettingsDisplayDriver>();

        services.AddDisplayDriver<AuditTrailIndexOptions, AuditTrailOptionsDisplayDriver>();

        services.AddScoped<IAuditTrailAdminListQueryService, DefaultAuditTrailAdminListQueryService>();

        services.AddSingleton<IAuditTrailAdminListFilterParser>(sp =>
        {
            var filterProviders = sp.GetServices<IAuditTrailAdminListFilterProvider>();
            var builder = new QueryEngineBuilder<AuditTrailEvent>();
            foreach (var provider in filterProviders)
            {
                provider.Build(builder);
            }

            var parser = builder.Build();

            return new DefaultAuditTrailAdminListFilterParser(parser);
        });

        services.AddTransient<IAuditTrailAdminListFilterProvider, DefaultAuditTrailAdminListFilterProvider>();

        services.AddOptions<AuditTrailOptions>();

        services.Configure<AuditTrailAdminListOptions>(options =>
        {
            options
                .ForSort("time-desc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.CreatedUtc))
                    .WithSelectListItem<Startup>((S, opt, model) => new SelectListItem(S["Newest"], opt.Value, model.Sort == string.Empty))
                    .AsDefault())

                .ForSort("time-asc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.CreatedUtc))
                    .WithSelectListItem<Startup>((S, opt, model) => new SelectListItem(S["Oldest"], opt.Value, model.Sort == opt.Value)))

                .ForSort("category-asc-time-desc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.Category).ThenByDescending(i => i.CreatedUtc))
                    .WithSelectListItem<Startup>((S, opt, model) => new SelectListItem(S["Category"], opt.Value, model.Sort == opt.Value)))

                .ForSort("category-asc-time-asc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.Category).ThenBy(i => i.CreatedUtc)))

                .ForSort("category-desc-time-desc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.Category).ThenByDescending(i => i.CreatedUtc)))

                .ForSort("category-desc-time-asc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.Category).ThenBy(i => i.CreatedUtc)))

                .ForSort("event-asc-time-desc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.Name).ThenByDescending(i => i.CreatedUtc))
                    .WithSelectListItem<Startup>((S, opt, model) => new SelectListItem(S["Event"], opt.Value, model.Sort == opt.Value)))

                .ForSort("event-asc-time-asc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.Name).ThenBy(i => i.CreatedUtc)))

                .ForSort("event-desc-time-desc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.Name).ThenByDescending(i => i.CreatedUtc)))

                .ForSort("event-desc-time-asc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.Name).ThenBy(i => i.CreatedUtc)))

                .ForSort("user-asc-time-asc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.NormalizedUserName).ThenBy(i => i.CreatedUtc))
                    .WithSelectListItem<Startup>((S, opt, model) => new SelectListItem(S["User"], opt.Value, model.Sort == opt.Value)))

                .ForSort("user-desc-time-desc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.NormalizedUserName).ThenByDescending(i => i.CreatedUtc)))

                .ForSort("user-desc-time-asc", b => b
                    .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.NormalizedUserName).ThenBy(i => i.CreatedUtc)));
        });
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<AuditTrailSettings, DeploymentStartup>(S => S["Audit Trail settings"], S => S["Exports the audit trail settings."]);
        services.AddSiteSettingsPropertyDeploymentStep<AuditTrailTrimmingSettings, DeploymentStartup>(S => S["Audit Trail Trimming settings"], S => S["Exports the audit trail trimming settings."]);
    }
}
