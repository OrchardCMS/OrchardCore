using Microsoft.Framework.DependencyInjection;
using System;
using Orchard.Hosting.Descriptor.Models;
using System.Linq;
using Orchard.Configuration.Environment;
using Microsoft.Framework.Logging;
using System.Diagnostics;

namespace Orchard.Hosting.ShellBuilders {
    /// <summary>
    /// High-level coordinator that exercises other component capabilities to
    /// build all of the artifacts for a running shell given a tenant settings.
    /// </summary>
    public interface IShellContextFactory {
        /// <summary>
        /// Builds a shell context given a specific tenant settings structure
        /// </summary>
        ShellContext CreateShellContext(ShellSettings settings);

        /// <summary>
        /// Builds a shell context for an uninitialized Orchard instance. Needed
        /// to display setup user interface.
        /// </summary>
        ShellContext CreateSetupContext(ShellSettings settings);
    }

    public class ShellContextFactory : IShellContextFactory {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ILogger _logger;

        public ShellContextFactory(
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory,
            ILoggerFactory loggerFactory) {
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
            _logger = loggerFactory.CreateLogger<ShellContextFactory>();
        }

        ShellContext IShellContextFactory.CreateShellContext(
            ShellSettings settings) {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("Creating shell context for tenant {0}", settings.Name);

            var blueprint = _compositionStrategy.Compose(settings, MinimumShellDescriptor());
            var provider = _shellContainerFactory.CreateContainer(settings, blueprint);

            try {
                return new ShellContext {
                    Settings = settings,
                    Blueprint = blueprint,
                    LifetimeScope = provider,
                    Shell = provider.GetRequiredService<IOrchardShell>()
                };
            }
            catch (Exception ex) {
                _logger.LogError("Cannot create shell context", ex);
                throw;
            }
            finally {
                _logger.LogVerbose("Created shell context for tenant {0} in {1}ms", settings.Name, sw.ElapsedMilliseconds);
            }
        }

        private static ShellDescriptor MinimumShellDescriptor() {
            return new ShellDescriptor {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature { Name = "Orchard.Logging.Console" },
                    new ShellFeature { Name = "Orchard.Hosting" },
                    new ShellFeature { Name = "Settings" },
                    new ShellFeature { Name = "Orchard.Demo" },
                    new ShellFeature { Name = "Orchard.Data.EntityFramework" },
                    new ShellFeature { Name = "Orchard.Data.EntityFramework.InMemory" },
                    new ShellFeature { Name = "Orchard.Data.EntityFramework.Indexing" }
                },
                Parameters = Enumerable.Empty<ShellParameter>(),
            };
        }

        ShellContext IShellContextFactory.CreateSetupContext(ShellSettings settings) {
            _logger.LogDebug("No shell settings available. Creating shell context for setup");

            var descriptor = new ShellDescriptor {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature { Name = "Orchard.Logging.Console" },
                    new ShellFeature { Name = "Orchard.Setup" },
                },
            };

            var blueprint = _compositionStrategy.Compose(settings, descriptor);
            var provider = _shellContainerFactory.CreateContainer(settings, blueprint);

            return new ShellContext {
                Settings = settings,
                Blueprint = blueprint,
                LifetimeScope = provider,
                Shell = provider.GetService<IOrchardShell>()
            };
        }
    }
}