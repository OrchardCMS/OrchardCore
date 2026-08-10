using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Json;
using OrchardCore.Modules;
using OrchardCore.SignalR.Middlewares;
using OrchardCore.SignalR.Services;

namespace OrchardCore.SignalR;

/// <summary>
/// Registers SignalR, the SignalR client resources, and hub authentication.
/// </summary>
public sealed class Startup : StartupBase
{
    // The hub authentication middleware must run after the authentication middleware and before any
    // module that authorizes hub endpoints.
    public override int ConfigureOrder
        => OrchardCoreConstants.ConfigureOrder.Authentication + 1;

    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                foreach (var converter in JOptions.KnownConverters)
                {
                    options.PayloadSerializerOptions.Converters.Add(converter);
                }
            });

        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.UseMiddleware<SignalRAuthenticationMiddleware>();
    }
}
