using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OrchardCore.Modules;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration.Records;
using OrchardCore.Environment.Extensions;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Data.Migration
{
    public class DataMigrationManager : IDataMigrationManager
    {
        private readonly IEnumerable<IDataMigration> _dataMigrations;
        private readonly ISession _session;
        private readonly IStore _store;
        private readonly IExtensionManager _extensionManager;
        private readonly ILogger _logger;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        private readonly List<string> _processedFeatures;
        private DataMigrationRecord _dataMigrationRecord;

        public DataMigrationManager(
            ITypeFeatureProvider typeFeatureProvider,
            IEnumerable<IDataMigration> dataMigrations,
            ISession session,
            IStore store,
            IExtensionManager extensionManager,
            ILogger<DataMigrationManager> logger,
            IStringLocalizer<DataMigrationManager> localizer)
        {
            _typeFeatureProvider = typeFeatureProvider;
            _dataMigrations = dataMigrations;
            _session = session;
            _store = store;
            _extensionManager = extensionManager;
            _logger = logger;

            _processedFeatures = new List<string>();

            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public async Task<DataMigrationRecord> GetDataMigrationRecordAsync()
        {
            if (_dataMigrationRecord == null)
            {
                _dataMigrationRecord = await _session.Query<DataMigrationRecord>().FirstOrDefaultAsync();

                if (_dataMigrationRecord == null)
                {
                    _dataMigrationRecord = new DataMigrationRecord();
                    _session.Save(_dataMigrationRecord);
                }
            }

            return _dataMigrationRecord;
        }

        public async Task<IEnumerable<string>> GetFeaturesThatNeedUpdateAsync()
        {
            var currentVersions = (await GetDataMigrationRecordAsync()).DataMigrations
                .ToDictionary(r => r.DataMigrationClass);

            var outOfDateMigrations = _dataMigrations.Where(dataMigration =>
            {
                Records.DataMigration record;
                if (currentVersions.TryGetValue(dataMigration.GetType().FullName, out record) && record.Version.HasValue)
                {
                    return CreateUpgradeLookupTable(dataMigration).ContainsKey(record.Version.Value);
                }

                return (GetCreateMethod(dataMigration) != null);
            });

            return outOfDateMigrations.Select(m => _typeFeatureProvider.GetFeatureForDependency(m.GetType()).Id).ToList();
        }

        public async Task Uninstall(string feature)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Uninstalling feature '{FeatureName}'.", feature);
            }
            var migrations = GetDataMigrations(feature);

            // apply update methods to each migration class for the module
            foreach (var migration in migrations)
            {
                // copy the object for the Linq query
                var tempMigration = migration;

                // get current version for this migration
                var dataMigrationRecord = await GetDataMigrationRecordAsync(tempMigration);

                var uninstallMethod = GetUninstallMethod(migration);
                if (uninstallMethod != null)
                {
                    uninstallMethod.Invoke(migration, new object[0]);
                }

                if (dataMigrationRecord == null)
                {
                    continue;
                }

                (await GetDataMigrationRecordAsync()).DataMigrations.Remove(dataMigrationRecord);
            }
        }

        public async Task UpdateAsync(IEnumerable<string> featureIds)
        {
            foreach (var featureId in featureIds)
            {
                if (!_processedFeatures.Contains(featureId))
                {
                    await UpdateAsync(featureId);
                }
            }
        }

        public async Task UpdateAsync(string featureId)
        {
            if (_processedFeatures.Contains(featureId))
            {
                return;
            }

            _processedFeatures.Add(featureId);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Updating feature '{FeatureName}'", featureId);
            }

            // proceed with dependent features first, whatever the module it's in
            var dependencies = _extensionManager
                .GetDependentFeatures(
                    featureId)
                .Where(x => x.Id != featureId)
                .Select(x => x.Id);

            await UpdateAsync(dependencies);

            var migrations = GetDataMigrations(featureId);

            // apply update methods to each migration class for the module
            foreach (var migration in migrations)
            {
                var schemaBuilder = new SchemaBuilder(_session);
                migration.SchemaBuilder = schemaBuilder;

                // copy the object for the Linq query
                var tempMigration = migration;

                // get current version for this migration
                var dataMigrationRecord = await GetDataMigrationRecordAsync(tempMigration);

                var current = 0;
                if (dataMigrationRecord != null)
                {
                    current = dataMigrationRecord.Version.Value;
                }
                else
                {
                    dataMigrationRecord = new Records.DataMigration { DataMigrationClass = migration.GetType().FullName };
                    _dataMigrationRecord.DataMigrations.Add(dataMigrationRecord);
                }

                try
                {
                    // do we need to call Create() ?
                    if (current == 0)
                    {
                        // try to resolve a Create method

                        var createMethod = GetCreateMethod(migration);
                        if (createMethod != null)
                        {
                            current = (int)createMethod.Invoke(migration, new object[0]);
                        }
                    }

                    var lookupTable = CreateUpgradeLookupTable(migration);

                    while (lookupTable.ContainsKey(current))
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Applying migration for '{FeatureName}' from version {Version}.", featureId, current);
                        }

                        current = (int)lookupTable[current].Invoke(migration, new object[0]);
                    }

                    // if current is 0, it means no upgrade/create method was found or succeeded
                    if (current == 0)
                    {
                        return;
                    }

                    dataMigrationRecord.Version = current;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running migration version {Version} for '{FeatureName}'.", current, featureId);

                    _session.Cancel();
                }
                finally
                {
                    // Persist data migrations
                    _session.Save(_dataMigrationRecord);
                }
            }
        }

        private async Task<Records.DataMigration> GetDataMigrationRecordAsync(IDataMigration tempMigration)
        {
            var dataMigrationRecord = await GetDataMigrationRecordAsync();
            return dataMigrationRecord
                .DataMigrations
                .FirstOrDefault(dm => dm.DataMigrationClass == tempMigration.GetType().FullName);
        }

        /// <summary>
        /// Returns all the available IDataMigration instances for a specific module, and inject necessary builders
        /// </summary>
        private IEnumerable<IDataMigration> GetDataMigrations(string featureId)
        {
            var migrations = _dataMigrations
                    .Where(dm => _typeFeatureProvider.GetFeatureForDependency(dm.GetType()).Id == featureId)
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
        private static Dictionary<int, MethodInfo> CreateUpgradeLookupTable(IDataMigration dataMigration)
        {
            return dataMigration
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(GetUpdateMethod)
                .Where(tuple => tuple != null)
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        private static Tuple<int, MethodInfo> GetUpdateMethod(MethodInfo mi)
        {
            const string updatefromPrefix = "UpdateFrom";

            if (mi.Name.StartsWith(updatefromPrefix))
            {
                var version = mi.Name.Substring(updatefromPrefix.Length);
                int versionValue;
                if (int.TryParse(version, out versionValue))
                {
                    return new Tuple<int, MethodInfo>(versionValue, mi);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the Create method from a data migration class if it's found
        /// </summary>
        private static MethodInfo GetCreateMethod(IDataMigration dataMigration)
        {
            var methodInfo = dataMigration.GetType().GetMethod("Create", BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo != null && methodInfo.ReturnType == typeof(int))
            {
                return methodInfo;
            }

            return null;
        }

        /// <summary>
        /// Returns the Uninstall method from a data migration class if it's found
        /// </summary>
        private static MethodInfo GetUninstallMethod(IDataMigration dataMigration)
        {
            var methodInfo = dataMigration.GetType().GetMethod("Uninstall", BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo != null && methodInfo.ReturnType == typeof(void))
            {
                return methodInfo;
            }

            return null;
        }

        public async Task UpdateAllFeaturesAsync()
        {
            var featuresThatNeedUpdate = await GetFeaturesThatNeedUpdateAsync();

            foreach (var featureId in featuresThatNeedUpdate)
            {
                try
                {
                    await UpdateAsync(featureId);
                }
                catch (Exception ex)
                {
                    if (ex.IsFatal())
                    {
                        throw;
                    }

                    _logger.LogError(ex, "Could not run migrations automatically on '{FeatureName}'", featureId);
                }
            }
        }
    }
}