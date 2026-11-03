using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.SignalR.Azure;

/// <summary>
/// Registers the Azure SignalR Service backplane, using the connection string configured under the
/// <c>SignalR:Azure:ConnectionString</c> key.
/// </summary>
[Feature(AzureBackplaneFeature)]
public sealed class Startup : StartupBase
{
    private const string AzureBackplaneFeature = "OrchardCore.SignalR.Azure";
    private const string BackplaneRegistrationKey = "OrchardCore.SignalR.Backplane";
    private const string ConfigurationSection = "SignalR:Azure";
    private const string DefaultApplicationName = "OrchardCore";
    private const string RedisBackplaneFeature = "OrchardCore.SignalR.Redis";

    private readonly IShellConfiguration _configuration;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public Startup(
        IShellConfiguration configuration,
        ShellSettings shellSettings,
        ILogger<Startup> logger)
    {
        _configuration = configuration;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        var configurationSection = _configuration.GetSection(ConfigurationSection);
        var connectionString = configurationSection.GetValue<string>("ConnectionString");

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning(
                "The '{Feature}' feature is enabled but '{Section}:ConnectionString' is not configured.",
                AzureBackplaneFeature,
                ConfigurationSection);

            return;
        }

        _logger.LogInformation("The Azure SignalR Service backplane is enabled.");

        EnsureNoBackplaneIsRegistered(services);

        services
            .AddSignalR()
            .AddAzureSignalR(options =>
            {
                options.ConnectionString = connectionString;
                options.ApplicationName = CreateApplicationName(
                    configurationSection.GetValue<string>("ApplicationName"),
                    _shellSettings.Name);
            });
    }

    internal static string CreateApplicationName(string applicationName, string tenantName)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            applicationName = DefaultApplicationName;
        }

        var tenantHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tenantName)));

        return $"{applicationName}_{tenantHash}";
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
