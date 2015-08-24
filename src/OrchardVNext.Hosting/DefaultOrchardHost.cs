using Microsoft.Framework.Logging;
using OrchardVNext.Configuration.Environment;
using OrchardVNext.Hosting.ShellBuilders;

namespace OrchardVNext.Hosting {
    public class DefaultOrchardHost : IOrchardHost {
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
    }
}