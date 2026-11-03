using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Redis;
using StackExchange.Redis;
using SignalRRedisOptions = Microsoft.AspNetCore.SignalR.StackExchangeRedis.RedisOptions;

namespace OrchardCore.SignalR.Redis;

/// <summary>
/// Registers the tenant-qualified Redis backplane for SignalR.
/// </summary>
[Feature(RedisBackplaneFeature)]
public sealed class Startup : StartupBase
{
    private const string AzureBackplaneFeature = "OrchardCore.SignalR.Azure";
    private const string BackplaneRegistrationKey = "OrchardCore.SignalR.Backplane";
    private const string RedisBackplaneFeature = "OrchardCore.SignalR.Redis";

    public override void ConfigureServices(IServiceCollection services)
    {
        if (!services.Any(descriptor => descriptor.ServiceType == typeof(IRedisService)))
        {
            return;
        }

        EnsureNoBackplaneIsRegistered(services);

        services
            .AddSignalR()
            .AddStackExchangeRedis();

        services.AddTransient<IConfigureOptions<SignalRRedisOptions>, SignalRRedisOptionsConfiguration>();
    }

    private static void EnsureNoBackplaneIsRegistered(IServiceCollection services)
    {
        if (services.Any(descriptor =>
            descriptor.IsKeyedService &&
            string.Equals(descriptor.ServiceKey as string, BackplaneRegistrationKey, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"The '{AzureBackplaneFeature}' and '{RedisBackplaneFeature}' features cannot be enabled together.");
        }

        services.AddKeyedSingleton<object>(BackplaneRegistrationKey, new object());
    }
}

internal sealed class SignalRRedisOptionsConfiguration : IConfigureOptions<SignalRRedisOptions>
{
    private readonly RedisOptions _redisOptions;
    private readonly string _tenantName;

    public SignalRRedisOptionsConfiguration(IOptions<RedisOptions> redisOptions, ShellSettings shellSettings)
    {
        _redisOptions = redisOptions.Value;
        _tenantName = shellSettings.Name;
    }

    public void Configure(SignalRRedisOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // A dedicated channel prefix keeps every tenant's SignalR traffic isolated on a shared Redis instance.
        options.Configuration = _redisOptions.ConfigurationOptions.Clone();
        options.Configuration.ChannelPrefix = RedisChannel.Literal(
            $"{_redisOptions.InstancePrefix}{_tenantName}:SignalR");
    }
}
