using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Tenants.Azure.Services;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Azure
{
    [Feature("OrchardCore.Tenants.FileProvider.Azure")]
    public class Startup : StartupBase
    {
        private readonly ILogger _logger;
        private readonly IShellConfiguration _configuration;

        public Startup(ILogger<Startup> logger, IShellConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override int Order => 10;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TenantBlobStorageOptions>(_configuration.GetSection("OrchardCore.Tenants.FileProvider.Azure"));

            // Only replace default implementation if options are valid.
            var connectionString = _configuration[$"OrchardCore.Tenants.FileProvider.Azure:{nameof(TenantBlobStorageOptions.ConnectionString)}"];
            var containerName = _configuration[$"OrchardCore.Tenants.FileProvider.Azure:{nameof(TenantBlobStorageOptions.ContainerName)}"];
            if (CheckOptions(connectionString, containerName, _logger))
            {
                // Replace the default tenant file provider with BlobTenantFileProvider
                services.Replace(ServiceDescriptor.Singleton<ITenantFileProvider>(serviceProvider =>
                {
                    var storageOptions = serviceProvider.GetRequiredService<IOptions<TenantBlobStorageOptions>>().Value;
                    return new BlobTenantFileProvider(storageOptions);
                }));
            }
        }

        private static bool CheckOptions(string connectionString, string containerName, ILogger logger)
        {
            var optionsAreValid = true;

            if (String.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogError("Azure Tenant Storage is enabled but not active because the 'ConnectionString' is missing or empty in application configuration.");
                optionsAreValid = false;
            }

            if (String.IsNullOrWhiteSpace(containerName))
            {
                logger.LogError("Azure Tenant Storage is enabled but not active because the 'ContainerName' is missing or empty in application configuration.");
                optionsAreValid = false;
            }

            return optionsAreValid;
        }
    }
}
