using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Extensions;
using OrchardVNext.Environment.ShellBuilders;
using System;

namespace OrchardVNext.Environment {
    public interface IOrchardHost {
        void Initialize();

        ShellContext CreateShellContext(ShellSettings settings);
    }

    public class DefaultOrchardHost : IOrchardHost {
        private readonly IShellContextFactory _shellContextFactory;

        public DefaultOrchardHost(IShellContextFactory shellContextFactory) {
            _shellContextFactory = shellContextFactory;
        }

        void IOrchardHost.Initialize() {
            Logger.Information("Initialize Host");

            //_extensionLoaderCoordinator.SetupExtensions();

            Logger.Information("Host Initialized");
        }

        /// <summary>
        /// Creates a shell context based on shell settings
        /// </summary>
        public ShellContext CreateShellContext(ShellSettings settings) {
            if (settings.State == TenantState.Uninitialized) {
                Logger.Debug("Creating shell context for tenant {0} setup", settings.Name);
                return _shellContextFactory.CreateSetupContext(settings);
            }

            Logger.Debug("Creating shell context for tenant {0}", settings.Name);
            return _shellContextFactory.CreateShellContext(settings);
        }
    }
}