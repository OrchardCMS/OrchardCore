using Microsoft.Framework.DependencyInjection;
using System;
using OrchardVNext.Hosting.Descriptor.Models;
using System.Linq;
using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Hosting.ShellBuilders {
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

        public ShellContextFactory(
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory) {
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
        }

        ShellContext IShellContextFactory.CreateShellContext(
            ShellSettings settings) {
            Logger.Information("Creating shell context for tenant {0}", settings.Name);

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
                Logger.Error(ex.ToString());
                throw;
            }
        }

        private static ShellDescriptor MinimumShellDescriptor() {
            return new ShellDescriptor {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature {Name = "OrchardVNext.Hosting"},
                    new ShellFeature {Name = "Settings"},
                    new ShellFeature {Name = "OrchardVNext.Test1"},
                    new ShellFeature {Name = "OrchardVNext.Demo" },
                    new ShellFeature {Name = "OrchardVNext.Data.EntityFramework" }
                },
                Parameters = Enumerable.Empty<ShellParameter>(),
            };
        }

        ShellContext IShellContextFactory.CreateSetupContext(ShellSettings settings) {
            Logger.Debug("No shell settings available. Creating shell context for setup");

            var descriptor = new ShellDescriptor {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature { Name = "OrchardVNext.Setup" },
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