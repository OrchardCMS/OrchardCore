using OrchardVNext.Environment.Configuration;
using Microsoft.Framework.DependencyInjection;
using System;
using OrchardVNext.Environment.Descriptor.Models;
using System.Linq;

namespace OrchardVNext.Environment.ShellBuilders {
    /// <summary>
    /// High-level coordinator that exercises other component capabilities to
    /// build all of the artifacts for a running shell given a tenant settings.
    /// </summary>
    public interface IShellContextFactory {
        /// <summary>
        /// Builds a shell context given a specific tenant settings structure
        /// </summary>
        ShellContext CreateShellContext(ShellSettings settings);
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
            Console.WriteLine("Creating shell context for tenant {0}", settings.Name);

            var blueprint = _compositionStrategy.Compose(settings, MinimumShellDescriptor());
            var provider = _shellContainerFactory.CreateContainer(settings, blueprint);

            try {
                return new ShellContext {
                    Settings = settings,
                    Blueprint = blueprint,
                    LifetimeScope = provider,
                    Shell = provider.GetService<IOrchardShell>()
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
                    new ShellFeature {Name = "OrchardVNext.Framework"},
                    new ShellFeature {Name = "Settings"},
                    new ShellFeature {Name = "OrchardVNext.Test1"},
                    new ShellFeature {Name = "OrchardVNext.Demo" }
                },
                Parameters = Enumerable.Empty<ShellParameter>(),
            };
        }
    }
}