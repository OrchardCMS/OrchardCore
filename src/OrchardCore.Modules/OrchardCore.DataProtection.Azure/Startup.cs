using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            services
                .Configure<BlobOptions, BlobOptionsSetup>()
                .AddDataProtection()
                .PersistKeysToAzureBlobStorage(sp =>
                {
                    var options = sp.GetRequiredService<BlobOptions>();
                    return new BlobClient(
                        options.ConnectionString,
                        options.ContainerName,
                        options.BlobName);
                });

            services.AddScoped<IModularTenantEvents, BlobModularTenantEvents>();
        }
        else
        {
            _logger.LogCritical("No connection string was supplied for OrchardCore.DataProtection.Azure. Ensure that an application setting containing a valid Azure Storage connection string is available at `Modules:OrchardCore.DataProtection.Azure:ConnectionString`.");
        }
    }
}
