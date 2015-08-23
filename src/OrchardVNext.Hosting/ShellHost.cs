using Microsoft.Framework.Logging;
using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Hosting {
    public class ShellHost : IShellHost {
        private readonly ILogger _logger;

        public ShellHost(
            ILoggerFactory loggerFactory) {

            _logger = loggerFactory.CreateLogger<ShellHost>();
        }

        void IShellHost.BeginRequest(ShellSettings settings) {
            _logger.LogDebug("Begin Request for tenant {0}", settings.Name);
        }

        void IShellHost.EndRequest(ShellSettings settings) {
            _logger.LogDebug("End Request for tenant {0}", settings.Name);
        }
    }
}