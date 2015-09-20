using Microsoft.Framework.Logging;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Builders;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Environment.Shell.Models;
using Orchard.Hosting.ShellBuilders;

namespace Orchard.Hosting {
    public class DefaultOrchardHost : IOrchardHost, IShellDescriptorManagerEventHandler {
        private readonly IShellContextFactory _shellContextFactory;
        private readonly ILogger _logger;

        public DefaultOrchardHost(IShellContextFactory shellContextFactory,
            ILoggerFactory loggerFactory) {
            _shellContextFactory = shellContextFactory;
            _logger = loggerFactory.CreateLogger<DefaultOrchardHost>();
        }

        void IOrchardHost.Initialize() {
            _logger.LogInformation("Initialize Host");



            _logger.LogInformation("Host Initialized");
        }

        /// <summary>
        /// Creates a shell context based on shell settings
        /// </summary>
        public ShellContext CreateShellContext(ShellSettings settings) {
            if (settings.State == TenantState.Uninitialized) {
                _logger.LogDebug("Creating shell context for tenant {0} setup", settings.Name);
                return _shellContextFactory.CreateSetupContext(settings);
            }

            _logger.LogDebug("Creating shell context for tenant {0}", settings.Name);
            return _shellContextFactory.CreateShellContext(settings);
        }

        /// <summary>
        /// A feature is enabled/disabled, the tenant needs to be restarted
        /// </summary>
        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant) {
            _logger.LogDebug("Something changed! ARGH! for tenant {0}", tenant);
        }
    }
}