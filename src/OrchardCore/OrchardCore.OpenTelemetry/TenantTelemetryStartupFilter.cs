using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace OrchardCore.Telemetry;

/// <summary>
/// Adds the <see cref="TenantTelemetryMiddleware"/> at the start of the application pipeline.
/// </summary>
internal sealed class TenantTelemetryStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<TenantTelemetryMiddleware>();
            next(app);
        };
    }
}
