using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
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
