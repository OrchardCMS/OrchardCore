using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.DataProtection.Azure;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _configuration;
    private readonly ILogger _logger;

    public Startup(IShellConfiguration configuration, ILogger<Startup> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public override int Order
        => OrchardCoreConstants.ConfigureOrder.AzureDataProtection;

    public override void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration.GetValue<string>("OrchardCore_DataProtection_Azure:ConnectionString");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("OrchardCore_DataProtection_Azure:ConnectionString was found. Adding 'OrchardCore.DataProtection.Azure' feature services.");
            }

            // Remove any previously registered options setups.
            services.RemoveAll<IConfigureOptions<KeyManagementOptions>>();

            services
                .AddDataProtection()
                .PersistKeysToAzureBlobStorage(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<BlobOptions>>().Value;

                    var logger = sp.GetRequiredService<ILogger<Startup>>();

                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Creating BlobClient instance using '{ContainerName}' as container name and '{BlobName}' as blob name.", options.ContainerName, options.BlobName);
                    }

                    return new BlobClient(
                        options.ConnectionString,
                        options.ContainerName,
                        options.BlobName);
                });

            services.AddSingleton<IConfigureOptions<BlobOptions>, BlobOptionsConfiguration>();

            services.AddScoped<IModularTenantEvents, BlobModularTenantEvents>();
        }
        else
        {
            _logger.LogCritical("No connection string was supplied for OrchardCore.DataProtection.Azure. Ensure that an application setting containing a valid Azure Storage connection string is available at `OrchardCore:OrchardCore_DataProtection_Azure:ConnectionString`.");
        }
    }
}
