using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Hosting.ShellBuilders;
using System;
using System.Collections.Generic;

namespace Orchard.Environment.Shell.Builders
{
    public class ShellContextFactory : IShellContextFactory
    {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ILogger _logger;

        public ShellContextFactory(
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory,
            ILogger<ShellContextFactory> logger)
        {
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
            
            var knownDescriptor = MinimumShellDescriptor();
            return CreateDescribedContext(settings, knownDescriptor);
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
                },
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
                Features = new[] {
                    new ShellFeature { Name = "Orchard.Logging.Console" },
                    new ShellFeature { Name = "Orchard.Hosting" },
                    new ShellFeature { Name = "Settings" },
                    new ShellFeature { Name = "Dashboard" },
                    new ShellFeature { Name = "Title" },
                    new ShellFeature { Name = "Navigation" },
                    new ShellFeature { Name = "Orchard.Themes" },
                    new ShellFeature { Name = "Orchard.Contents" },
                    new ShellFeature { Name = "Orchard.Lists" },
                    new ShellFeature { Name = "Orchard.ContentTypes" },
                    new ShellFeature { Name = "Orchard.Demo" },
                    new ShellFeature { Name = "Orchard.DynamicCache" },
                    new ShellFeature { Name = "TheTheme" },
                    new ShellFeature { Name = "TheAdmin" },
                    new ShellFeature { Name = "SafeMode" },
                },
                Parameters = new List<ShellParameter>()
            };
        }

    }
}
