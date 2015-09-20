using Microsoft.Framework.Logging;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell;
using System;

namespace Orchard.Data.Migration {
    /// <summary>
    /// Registers to OrchardShell.Activated in order to run migrations automatically 
    /// </summary>
    public class AutomaticDataMigrations : IOrchardShellEvents {
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IFeatureManager _featureManager;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public AutomaticDataMigrations(
            IDataMigrationManager dataMigrationManager,
            IFeatureManager featureManager,
            ShellSettings shellSettings,
            ILoggerFactory loggerFactory) {

            _dataMigrationManager = dataMigrationManager;
            _featureManager = featureManager;
            _shellSettings = shellSettings;
            _logger = loggerFactory.CreateLogger<AutomaticDataMigrations>();
        }

        public void Activated() {
            foreach (var feature in _dataMigrationManager.GetFeaturesThatNeedUpdate()) {
                try {
                    _dataMigrationManager.Update(feature);
                }
                catch (Exception ex) {
                    if (ex.IsFatal()) {
                        throw;
                    }
                    _logger.LogError("Could not run migrations automatically on " + feature, ex);
                }
            }
        }

        public void Terminating() {
        }
    }
}
