using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.SignalR;

/// <summary>
/// Registers the Azure SignalR Service backplane, using the connection string configured under the
/// <c>SignalR:Azure:ConnectionString</c> key.
/// </summary>
[Feature(SignalRConstants.Feature.AzureBackplane)]
public sealed class AzureBackplaneStartup : StartupBase
{
    private readonly IShellConfiguration _configuration;
    private readonly ILogger _logger;

    public AzureBackplaneStartup(
        IShellConfiguration configuration,
        ILogger<AzureBackplaneStartup> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration
            .GetSection("SignalR:Azure")
            .GetValue<string>("ConnectionString");

        if (!string.IsNullOrEmpty(connectionString))
        {
            _logger.LogInformation("The Azure SignalR Service backplane is enabled.");

            services
                .AddSignalR()
                .AddAzureSignalR(connectionString);
        }
        else
        {
            _logger.LogWarning(
                "The '{Feature}' feature is enabled but 'SignalR:Azure:ConnectionString' is not configured.",
                SignalRConstants.Feature.AzureBackplane);
        }
    }
}
