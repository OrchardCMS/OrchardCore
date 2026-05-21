using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;
using OrchardCore.Users.AuditTrail.Drivers;
using OrchardCore.Users.AuditTrail.Handlers;
using OrchardCore.Users.AuditTrail.Indexes;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.Security;
using OrchardCore.Users.AuditTrail.Services;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;

namespace OrchardCore.Users.AuditTrail;

[Feature("OrchardCore.Users.AuditTrail")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<AuditTrailOptions>, UserAuditTrailEventConfiguration>();

        services.AddScoped<UserEventHandler, UserEventHandler>()
            .AddScoped<IUserEventHandler>(sp => sp.GetRequiredService<UserEventHandler>())
            .AddScoped<ILoginFormEvent>(sp => sp.GetRequiredService<UserEventHandler>());

        services.AddDisplayDriver<AuditTrailEvent, AuditTrailUserEventDisplayDriver>();
        services.AddIndexProvider<AuditTrailUserEventIndexProvider>();
        services.AddDataMigration<Migrations>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddScoped<IAuthorizationHandler, ViewUserAuditTrailEventsHandler>();

        services.AddSingleton<Redactor>(_ => NullRedactor.Instance);
        services.AddSingleton<Redactor>(_ => ErasingRedactor.Instance);
        services.AddSingleton<Redactor, TenantHmacRedactor>();
        services.AddSingleton<Redactor, RemoveRedactor>();
        services.AddSingleton<Redactor, PartialAsteriskRedactor>();
        
        services.AddPermissionProvider<Permissions>();
    }
}

[RequireFeatures("OrchardCore.Users.AuditTrail", "OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddSiteSettingsPropertyDeploymentStep<AuditTrailUserEventSettings, DeploymentStartup>(
            S => S["User Audit Trail settings"],
            S => S["Exports the user audit trail settings."]);
}
