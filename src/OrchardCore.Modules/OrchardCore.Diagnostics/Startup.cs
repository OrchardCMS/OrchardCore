using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Diagnostics;

public sealed class Startup : Modules.StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IStartupFilter, DiagnosticsStartupFilter>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "Diagnostics.Error",
            areaName: "OrchardCore.Diagnostics",
            pattern: "Error/{status?}",
            defaults: new { controller = "Diagnostics", action = "Error" }
        );
    }
}
