using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;

namespace Orchard.Hosting {
    public class DefaultOrchardShellHost : IOrchardShellHost {
        private readonly ILogger _logger;

        public DefaultOrchardShellHost(
            ILoggerFactory loggerFactory) {

            _logger = loggerFactory.CreateLogger<DefaultOrchardShellHost>();
        }

        void IOrchardShellHost.BeginRequest(ShellSettings settings) {
            _logger.LogDebug("Begin Request for tenant {0}", settings.Name);
        }

        void IOrchardShellHost.EndRequest(ShellSettings settings) {
            _logger.LogDebug("End Request for tenant {0}", settings.Name);
        }
    }
}