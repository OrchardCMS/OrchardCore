using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Tenant.Descriptor;
using OrchardCore.Tenant.Descriptor.Models;
using Orchard.Hosting.TenantBuilders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Tenant.Builders
{
    public class TenantContextFactory : ITenantContextFactory
    {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly ITenantContainerFactory _tenantContainerFactory;
        private readonly IEnumerable<TenantFeature> _tenantFeatures;
        private readonly ILogger _logger;

        public TenantContextFactory(
            ICompositionStrategy compositionStrategy,
            ITenantContainerFactory tenantContainerFactory,
            IEnumerable<TenantFeature> tenantFeatures,
            ILogger<TenantContextFactory> logger)
        {
            _compositionStrategy = compositionStrategy;
            _tenantContainerFactory = tenantContainerFactory;
            _tenantFeatures = tenantFeatures;
            _logger = logger;
        }

        async Task<TenantContext> ITenantContextFactory.CreateTenantContextAsync(TenantSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating tenant context for tenant {0}", settings.Name);
            }

            var describedContext = await CreateDescribedContextAsync(settings, MinimumTenantDescriptor());

            TenantDescriptor currentDescriptor;
            using (var scope = describedContext.CreateServiceScope())
            {
                var tenantDescriptorManager = scope.ServiceProvider.GetService<ITenantDescriptorManager>();
                currentDescriptor = await tenantDescriptorManager.GetTenantDescriptorAsync();
            }

            if (currentDescriptor != null)
            {
                return await CreateDescribedContextAsync(settings, currentDescriptor);
            }

            return describedContext;
        }

        // TODO: This should be provided by a ISetupService that returns a set of TenantFeature instances.
        async Task<TenantContext> ITenantContextFactory.CreateSetupContextAsync(TenantSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("No tenant settings available. Creating tenant context for setup");
            }
            var descriptor = MinimumTenantDescriptor();

            return await CreateDescribedContextAsync(settings, descriptor);
        }

        public async Task<TenantContext> CreateDescribedContextAsync(TenantSettings settings, TenantDescriptor tenantDescriptor)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating described context for tenant {0}", settings.Name);
            }

            var blueprint = await _compositionStrategy.ComposeAsync(settings, tenantDescriptor);
            var provider = _tenantContainerFactory.CreateContainer(settings, blueprint);

            return new TenantContext
            {
                Settings = settings,
                Blueprint = blueprint,
                ServiceProvider = provider
            };
        }

        /// <summary>
        /// The minimum tenant descriptor is used to bootstrap the first container that will be used
        /// to call all module IStartup implementation. It's composed of module names that reference
        /// core components necessary for the desired scenario.
        /// </summary>
        /// <returns></returns>
        private TenantDescriptor MinimumTenantDescriptor()
        {
            // Load default features from the list of registered TenantFeature instances in the DI

            return new TenantDescriptor
            {
                SerialNumber = -1,
                Features = new List<TenantFeature>(_tenantFeatures),
                Parameters = new List<TenantParameter>()
            };
        }

    }
}
