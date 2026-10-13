using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Options;
using OrchardCore.Https.Drivers;
using OrchardCore.Https.Migrations;
using OrchardCore.Https.Services;
using OrchardCore.Https.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.Https;

public sealed class Startup : StartupBase
{
    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        // The HTTPS redirection and HSTS follow the HTTPS settings, which are updated without releasing the tenant.
        var httpsSettings = serviceProvider.GetRequiredService<IOptionsMonitor<HttpsSettings>>();
        var httpsRedirectionOptions = serviceProvider.GetRequiredService<IOptionsMonitor<HttpsRedirectionOptions>>();
        var hstsOptions = serviceProvider.GetRequiredService<IOptions<HstsOptions>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();

        // As 'UseHttpsRedirection()', use the server addresses to infer the HTTPS port when it is not configured.
        var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();

        app.Use(next => new HttpsPolicyMiddleware(
            next,
            httpsSettings,
            httpsRedirectionOptions,
            hstsOptions,
            configuration,
            loggerFactory,
            hostEnvironment,
            serverAddressesFeature).Invoke);
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteDisplayDriver<HttpsSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddSingleton<IHttpsService, HttpsService>();
        services.AddDataMigration<HttpsSettingsMigrations>();

        services.AddPermissionProvider<Permissions>();

        services.AddSignalOptionsChangeTokenSource<HttpsSettings>();
        services.AddSignalOptionsChangeTokenSource<HttpsRedirectionOptions>();
        services.AddTransient<IConfigureOptions<HttpsSettings>, HttpsSettingsConfiguration>();

        services.AddOptions<HttpsRedirectionOptions>()
            .Configure<IHttpsService>((options, service) =>
            {
                var settings = service.GetSettingsAsync().GetAwaiter().GetResult();
                if (settings.RequireHttpsPermanent)
                {
                    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                }

                options.HttpsPort = settings.SslPort;
            });

        services.AddHsts(options =>
        {
            options.Preload = false;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<HttpsSettings, DeploymentStartup>(S => S["Https settings"], S => S["Exports the Https settings."]);
    }
}
