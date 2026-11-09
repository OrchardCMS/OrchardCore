using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
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
        var validationErrors = _configuration.TryBindConfiguration(out var azureOptions)
            ? azureOptions.Validate()
            : [new ValidationResult("The 'SignalR:Azure' section is invalid.")];

        if (validationErrors.Any())
        {
            _logger.LogWarning(
                "The '{Feature}' feature is enabled but not active: {Errors}",
                AzureBackplaneFeature,
                string.Join(' ', validationErrors.Select(error => error.ErrorMessage)));
        }
        else if (!RegisterBackplane(services))
        {
            _logger.LogError(
                "The '{Feature}' feature is enabled but not active as the '{OtherFeature}' backplane is already registered. Enable only one SignalR backplane per tenant.",
                AzureBackplaneFeature,
                RedisBackplaneFeature);
        }
        else
        {
            _logger.LogInformation("The Azure SignalR Service backplane is enabled.");

            services
                .AddSignalR()
                .AddAzureSignalR(options =>
                {
                    options.ConnectionString = azureOptions.ConnectionString;
                    options.ApplicationName = CreateApplicationName(azureOptions.ApplicationName, _shellSettings.Name);
                });
        }
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

    private static bool RegisterBackplane(IServiceCollection services)
    {
        if (services.Any(descriptor =>
            descriptor.IsKeyedService &&
            string.Equals(descriptor.ServiceKey as string, BackplaneRegistrationKey, StringComparison.Ordinal)))
        {
            return false;
        }

        services.AddKeyedSingleton<object>(BackplaneRegistrationKey, new object());

        return true;
    }
}
