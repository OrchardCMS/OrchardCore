using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.SignalR.Azure;

/// <summary>
/// Registers the Azure SignalR Service backplane, using the connection string configured under the
/// <c>SignalR:Azure:ConnectionString</c> key.
/// </summary>
[Feature(SignalRConstants.Feature.AzureBackplane)]
public sealed class Startup : StartupBase
{
    private const string ConfigurationSection = "SignalR:Azure";

    private readonly IShellConfiguration _configuration;
    private readonly ILogger _logger;

    public Startup(
        IShellConfiguration configuration,
        ILogger<Startup> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration
            .GetSection(ConfigurationSection)
            .GetValue<string>("ConnectionString");

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning(
                "The '{Feature}' feature is enabled but '{Section}:ConnectionString' is not configured.",
                SignalRConstants.Feature.AzureBackplane,
                ConfigurationSection);

            return;
        }

        _logger.LogInformation("The Azure SignalR Service backplane is enabled.");

        services
            .AddSignalR()
            .AddAzureSignalR(connectionString);
    }
}
