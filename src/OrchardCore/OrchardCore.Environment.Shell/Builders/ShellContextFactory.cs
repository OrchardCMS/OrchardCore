using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Hosting.ShellBuilders;

namespace OrchardCore.Environment.Shell.Builders
{
    public class ShellContextFactory : IShellContextFactory
    {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly IEnumerable<ShellFeature> _shellFeatures;
        private readonly IEnumerable<CommonShellFeature> _commonShellFeatures;
        private readonly ILogger _logger;

        public ShellContextFactory(
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory,
            IEnumerable<ShellFeature> shellFeatures,
            IEnumerable<CommonShellFeature> commonShellFeatures,
            ILogger<ShellContextFactory> logger)
        {
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
            _shellFeatures = shellFeatures;
            _commonShellFeatures = commonShellFeatures;
            _logger = logger;
        }

        async Task<ShellContext> IShellContextFactory.CreateShellContextAsync(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating shell context for tenant {0}", settings.Name);
            }

            var describedContext = await CreateDescribedContextAsync(settings, CommonShellDescriptor());

            ShellDescriptor currentDescriptor;
            using (var scope = describedContext.EnterServiceScope())
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
                Features = new List<ShellFeature>(_shellFeatures.Union(_commonShellFeatures)),
                Parameters = new List<ShellParameter>()
            };
        }

        /// <summary>
        /// The common shell descriptor is used to bootstrap a temporary container that will be used
        /// to retrieve the current shell descriptor.
        /// </summary>
        private ShellDescriptor CommonShellDescriptor()
        {
            // Load common features from the list of registered CommonShellFeature instances in the DI

            return new ShellDescriptor
            {
                SerialNumber = -1,
                Features = new List<ShellFeature>(_commonShellFeatures),
                Parameters = new List<ShellParameter>()
            };
        }

    }
}
