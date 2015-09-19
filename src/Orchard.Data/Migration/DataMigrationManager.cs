using Microsoft.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Data.Migration {
    public class DataMigrationManager : IDataMigrationManager {
        private readonly ILogger _logger;

        public DataMigrationManager(
            ILoggerFactory loggerFactory) {
            
            _logger = loggerFactory.CreateLogger<DataMigrationManager>();
        }

        public IEnumerable<string> GetFeaturesThatNeedUpdate() {
            _logger.LogWarning("TODO: GetFeaturesThatNeedUpdate Feature");
            return Enumerable.Empty<string>();
        }

        public bool IsFeatureAlreadyInstalled(string feature) {
            _logger.LogWarning("TODO: IsFeatureAlreadyInstalled Feature");
            return false;
        }

        public void Uninstall(string feature) {
            _logger.LogWarning("TODO: Uninstall Feature");
        }

        public void Update(IEnumerable<string> features) {
            _logger.LogWarning("TODO: Update Feature");
        }

        public void Update(string feature) {
            _logger.LogWarning("TODO: Update Feature");
        }
    }
}
