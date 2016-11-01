using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Hosting.ShellBuilders;
using System.Collections.Generic;

namespace Orchard.Environment.Shell.Builders
{
    public class ShellContextFactory : IShellContextFactory
    {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ILogger _logger;
        private readonly IEnumerable<ShellFeature> _shellFeatures;

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

        ShellContext IShellContextFactory.CreateShellContext(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating shell context for tenant {0}", settings.Name);
            }

            var describedContext = CreateDescribedContext(settings, MinimumShellDescriptor());

            ShellDescriptor currentDescriptor;
            using (var scope = describedContext.CreateServiceScope())
            {
                var shellDescriptorManager = scope.ServiceProvider.GetService<IShellDescriptorManager>();
                currentDescriptor = shellDescriptorManager.GetShellDescriptorAsync().Result;
            }

            if (currentDescriptor != null)
            {
                return CreateDescribedContext(settings, currentDescriptor);
            }

            return describedContext;
        }

        // TODO: This should be provided by a ISetupService that returns a set of ShellFeature instances.
        ShellContext IShellContextFactory.CreateSetupContext(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("No shell settings available. Creating shell context for setup");
            }
            var descriptor = new ShellDescriptor
            {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature { Name = "Orchard.Setup" },
                    new ShellFeature { Name = "Orchard.Recipes" }
                }
            };

            return CreateDescribedContext(settings, descriptor);
        }

        public ShellContext CreateDescribedContext(ShellSettings settings, ShellDescriptor shellDescriptor)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating described context for tenant {0}", settings.Name);
            }

            var blueprint = _compositionStrategy.Compose(settings, shellDescriptor);
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
