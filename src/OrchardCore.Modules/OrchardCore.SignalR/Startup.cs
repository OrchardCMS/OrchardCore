using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.SignalR.Services;

namespace OrchardCore.SignalR;

/// <summary>
/// Registers SignalR and the SignalR client resources.
/// </summary>
public sealed class Startup : StartupBase
{
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

        services.AddTransient<IPostConfigureOptions<AuthorizationOptions>, AuthorizationOptionsConfiguration>();

        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
    }
}

[Feature("OrchardCore.SignalR.Core")]
public sealed class CoreStartup : StartupBase
{
    public override int ConfigureOrder
        => OrchardCoreConstants.ConfigureOrder.Authentication + 1;

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.UseMiddleware<AccessTokenHeaderMiddleware>();
    }
}
