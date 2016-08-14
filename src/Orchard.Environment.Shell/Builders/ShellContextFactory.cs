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
        private readonly IShellDescriptorCache _shellDescriptorCache;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ILogger _logger;

        public ShellContextFactory(
            IShellDescriptorCache shellDescriptorCache,
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory,
            ILogger<ShellContextFactory> logger)
        {
            _shellDescriptorCache = shellDescriptorCache;
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
            _logger = logger;
        }

        ShellContext IShellContextFactory.CreateShellContext(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating shell context for tenant {0}", settings.Name);
            }

            var knownDescriptor = _shellDescriptorCache.Fetch(settings.Name);
            if (knownDescriptor == null)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("No descriptor cached. Starting with minimum components.");
                }
                knownDescriptor = MinimumShellDescriptor();
            }

            var describedContext = CreateDescribedContext(settings, knownDescriptor);

            ShellDescriptor currentDescriptor;
            using (var scope = describedContext.CreateServiceScope())
            {
                var shellDescriptorManager = scope.ServiceProvider.GetService<IShellDescriptorManager>();
                currentDescriptor = shellDescriptorManager.GetShellDescriptorAsync().Result;
            }

            if (currentDescriptor != null && knownDescriptor.SerialNumber != currentDescriptor.SerialNumber)
            {
                _logger.LogInformation("Newer descriptor obtained. Rebuilding shell container.");

                _shellDescriptorCache.Store(settings.Name, currentDescriptor);
                return CreateDescribedContext(settings, currentDescriptor);
            }

            return describedContext;
        }

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
                    new ShellFeature { Name = "Orchard.Logging.Console" },
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

        private static ShellDescriptor MinimumShellDescriptor()
        {
            return new ShellDescriptor
            {
                SerialNumber = -1,
                Features = new[]
                {
                    new ShellFeature { Name = "Orchard.Logging.Console" },
                    new ShellFeature { Name = "Orchard.Hosting" },
                    new ShellFeature { Name = "Orchard.Admin" },
                    new ShellFeature { Name = "Orchard.Themes" },
                    new ShellFeature { Name = "TheAdmin" },
                    new ShellFeature { Name = "SafeMode" },
                    new ShellFeature { Name = "Orchard.Recipes" }
                },
                Parameters = new List<ShellParameter>()
            };
        }

    }
}
