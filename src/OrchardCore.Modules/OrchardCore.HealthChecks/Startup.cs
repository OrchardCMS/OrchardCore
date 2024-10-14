using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.HealthChecks.Services;
using OrchardCore.Modules;

namespace OrchardCore.HealthChecks;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IHealthChecksResponseWriter, DefaultHealthChecksResponseWriter>();
        services.AddHealthChecks();

        services.Configure<HealthChecksOptions>(_shellConfiguration.GetSection("OrchardCore_HealthChecks"));
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var healthChecksOptions = serviceProvider.GetService<IOptions<HealthChecksOptions>>().Value;

        if (healthChecksOptions.ShowDetails)
        {
            var healthChecksResponseWriter = serviceProvider.GetService<IHealthChecksResponseWriter>();

            app.UseHealthChecks(healthChecksOptions.Url, new HealthCheckOptions
            {
                AllowCachingResponses = false,
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status200OK
                },
                ResponseWriter = healthChecksResponseWriter.WriteResponseAsync
            });
        }
        else
        {
            app.UseHealthChecks(healthChecksOptions.Url);
        }
    }
}
