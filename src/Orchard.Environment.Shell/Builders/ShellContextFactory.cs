using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Hosting.ShellBuilders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Environment.Shell.Builders
{
    public class ShellContextFactory : IShellContextFactory
    {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly IEnumerable<ShellFeature> _shellFeatures;
        private readonly ILogger _logger;

        public ShellContextFactory(
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory,
            IEnumerable<ShellFeature> shellFeatures,
            ILogger<ShellContextFactory> logger)
        {
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
            _shellFeatures = shellFeatures;
            _logger = logger;
        }

        async Task<ShellContext> IShellContextFactory.CreateShellContextAsync(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating shell context for tenant {0}", settings.Name);
            }

            var describedContext = await CreateDescribedContextAsync(settings, MinimumShellDescriptor());

            ShellDescriptor currentDescriptor;
            using (var scope = describedContext.CreateServiceScope())
            {
                var shellDescriptorManager = scope.ServiceProvider.GetService<IShellDescriptorManager>();
                currentDescriptor = await shellDescriptorManager.GetShellDescriptorAsync();
            }

            if (currentDescriptor != null)
            {
                return await CreateDescribedContextAsync(settings, currentDescriptor);
            }

            return describedContext;
        }

        // TODO: This should be provided by a ISetupService that returns a set of ShellFeature instances.
        async Task<ShellContext> IShellContextFactory.CreateSetupContextAsync(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("No shell settings available. Creating shell context for setup");
            }
            var descriptor = MinimumShellDescriptor();

            return await CreateDescribedContextAsync(settings, descriptor);
        }

        public async Task<ShellContext> CreateDescribedContextAsync(ShellSettings settings, ShellDescriptor shellDescriptor)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating described context for tenant {0}", settings.Name);
            }

            var blueprint = await _compositionStrategy.ComposeAsync(settings, shellDescriptor);
            var provider = _shellContainerFactory.CreateContainer(settings, blueprint);

            return new ShellContext
            {
                Settings = settings,
                Blueprint = blueprint,
                ServiceProvider = provider
            };
        }

        /// <summary>
        /// The minimum shell descriptor is used to bootstrap the first container that will be used
        /// to call all module IStartup implementation. It's composed of module names that reference
        /// core components necessary for the desired scenario.
        /// </summary>
        /// <returns></returns>
        private ShellDescriptor MinimumShellDescriptor()
        {
            // Load default features from the list of registered ShellFeature instances in the DI

            return new ShellDescriptor
            {
                SerialNumber = -1,
                Features = new List<ShellFeature>(_shellFeatures),
                Parameters = new List<ShellParameter>()
            };
        }

    }
}
