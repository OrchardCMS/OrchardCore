using Microsoft.Extensions.Logging;
using Orchard.Data.Migration.Records;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orchard.Data.Migration {
    public class DataMigrationManager : IDataMigrationManager {
        private readonly IEnumerable<IDataMigration> _dataMigrations;
        private readonly IContentStorageManager _contentStorageManager;
        private readonly IExtensionManager _extensionManager;
        private readonly ILogger _logger;

        private readonly List<string> _processedFeatures;

        public DataMigrationManager(
            IEnumerable<IDataMigration> dataMigrations,
            IContentStorageManager contentStorageManager, 
            IExtensionManager extensionManager,
            ILoggerFactory loggerFactory) {
            _dataMigrations = dataMigrations;
            _contentStorageManager = contentStorageManager;
            _extensionManager = extensionManager;
            _logger = loggerFactory.CreateLogger<DataMigrationManager>();

            _processedFeatures = new List<string>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<string> GetFeaturesThatNeedUpdate() {
            var currentVersions = _contentStorageManager
                .Query<DataMigrationDocument>(x => x != null)
                .SelectMany(x => x.DataMigrationRecords)
                .ToDictionary(r => r.DataMigrationClass);

            var outOfDateMigrations = _dataMigrations.Where(dataMigration => {
                DataMigrationRecord record;
                if (currentVersions.TryGetValue(dataMigration.GetType().FullName, out record))
                    return CreateUpgradeLookupTable(dataMigration).ContainsKey(record.Version.Value);

                return (GetCreateMethod(dataMigration) != null);
            });

            return outOfDateMigrations.Select(m => m.Feature.Descriptor.Id).ToList();
        }

        /// <summary>
        /// Whether a feature has already been installed, i.e. one of its Data Migration class has already been processed
        /// </summary>
        public bool IsFeatureAlreadyInstalled(string feature) {
            return GetDataMigrations(feature).Any(dataMigration => GetDataMigrationRecord(dataMigration) != null);
        }

        public void Uninstall(string feature) {
            _logger.LogInformation("Uninstalling feature: {0}.", feature);

            var migrations = GetDataMigrations(feature);

            // apply update methods to each migration class for the module
            foreach (var migration in migrations) {
                // copy the object for the Linq query
                var tempMigration = migration;

                // get current version for this migration
                var dataMigrationRecord = GetDataMigrationRecord(tempMigration);

                var uninstallMethod = GetUninstallMethod(migration);
                if (uninstallMethod != null) {
                    uninstallMethod.Invoke(migration, new object[0]);
                }

                if (dataMigrationRecord == null) {
                    continue;
                }

                var record = _contentStorageManager.Query<DataMigrationDocument>(x => x != null).Single();
                record.DataMigrationRecords.Remove(dataMigrationRecord);
                _contentStorageManager.Store(record);
            }
        }

        public void Update(IEnumerable<string> features) {
            foreach (var feature in features) {
                if (!_processedFeatures.Contains(feature)) {
                    Update(feature);
                }
            }
        }

        public void Update(string feature) {
            _logger.LogWarning("TODO: Update Feature");
        }

        private DataMigrationRecord GetDataMigrationRecord(IDataMigration tempMigration) {
            return _contentStorageManager
                .Query<DataMigrationDocument>(x => x != null)
                .SelectMany(x => x.DataMigrationRecords)
                .FirstOrDefault(dm => dm.DataMigrationClass == tempMigration.GetType().FullName);
        }

        /// <summary>
        /// Returns all the available IDataMigration instances for a specific module, and inject necessary builders
        /// </summary>
        private IEnumerable<IDataMigration> GetDataMigrations(string feature) {
            var migrations = _dataMigrations
                    .Where(dm => string.Equals(dm.Feature.Descriptor.Id, feature, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            //foreach (var migration in migrations.OfType<DataMigrationImpl>()) {
            //    migration.SchemaBuilder = new SchemaBuilder(_interpreter, migration.Feature.Descriptor.Extension.Id, s => s.Replace(".", "_") + "_");
            //    migration.ContentDefinitionManager = _contentDefinitionManager;
            //}

            return migrations;
        }
        
        /// <summary>
        /// Create a list of all available Update methods from a data migration class, indexed by the version number
        /// </summary>
        private static Dictionary<int, MethodInfo> CreateUpgradeLookupTable(IDataMigration dataMigration) {
            return dataMigration
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(GetUpdateMethod)
                .Where(tuple => tuple != null)
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        private static Tuple<int, MethodInfo> GetUpdateMethod(MethodInfo mi) {
            const string updatefromPrefix = "UpdateFrom";

            if (mi.Name.StartsWith(updatefromPrefix)) {
                var version = mi.Name.Substring(updatefromPrefix.Length);
                int versionValue;
                if (int.TryParse(version, out versionValue)) {
                    return new Tuple<int, MethodInfo>(versionValue, mi);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the Create method from a data migration class if it's found
        /// </summary>
        private static MethodInfo GetCreateMethod(IDataMigration dataMigration) {
            var methodInfo = dataMigration.GetType().GetMethod("Create", BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo != null && methodInfo.ReturnType == typeof(int)) {
                return methodInfo;
            }

            return null;
        }

        /// <summary>
        /// Returns the Uninstall method from a data migration class if it's found
        /// </summary>
        private static MethodInfo GetUninstallMethod(IDataMigration dataMigration) {
            var methodInfo = dataMigration.GetType().GetMethod("Uninstall", BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo != null && methodInfo.ReturnType == typeof(void)) {
                return methodInfo;
            }

            return null;
        }
    }
}
