using Microsoft.Extensions.Logging;
using Orchard.Data.Migration.Records;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Data.Migration
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
            ILoggerFactory loggerFactory)
        {
            _typeFeatureProvider = typeFeatureProvider;
            _dataMigrations = dataMigrations;
            _session = session;
            _store = store;
            _extensionManager = extensionManager;
            _logger = loggerFactory.CreateLogger<DataMigrationManager>();

            _processedFeatures = new List<string>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public async Task<DataMigrationRecord> GetDataMigrationRecord()
        {
            if (_dataMigrationRecord == null)
            {
                _dataMigrationRecord = await _session
                .QueryAsync<DataMigrationRecord>()
                .FirstOrDefault();

                if (_dataMigrationRecord == null)
                {
                    _dataMigrationRecord = new DataMigrationRecord();
                    _session.Save(_dataMigrationRecord);
                }
            }

            return _dataMigrationRecord;
        }

        public async Task<IEnumerable<string>> GetFeaturesThatNeedUpdate()
        {
            var currentVersions = (await GetDataMigrationRecord()).DataMigrations
                .ToDictionary(r => r.DataMigrationClass);

            var outOfDateMigrations = _dataMigrations.Where(dataMigration =>
            {
                DataMigration record;
                if (currentVersions.TryGetValue(dataMigration.GetType().FullName, out record))
                    return CreateUpgradeLookupTable(dataMigration).ContainsKey(record.Version.Value);

                return (GetCreateMethod(dataMigration) != null);
            });

            return outOfDateMigrations.Select(m => _typeFeatureProvider.GetFeatureForDependency(m.GetType()).Descriptor.Id).ToList();
        }

        /// <summary>
        /// Whether a feature has already been installed, i.e. one of its Data Migration class has already been processed
        /// </summary>
        public bool IsFeatureAlreadyInstalled(string feature)
        {
            return GetDataMigrations(feature).Any(dataMigration => GetDataMigrationRecordAsync(dataMigration).Result != null);
        }

        public async Task Uninstall(string feature)
        {
            _logger.LogInformation("Uninstalling feature: {0}.", feature);

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

                (await GetDataMigrationRecord()).DataMigrations.Remove(dataMigrationRecord);
            }
        }

        public async Task UpdateAsync(IEnumerable<string> features)
        {
            foreach (var feature in features)
            {
                if (!_processedFeatures.Contains(feature))
                {
                    await UpdateAsync(feature);
                }
            }
        }

        public async Task UpdateAsync(string feature)
        {
            if (_processedFeatures.Contains(feature))
            {
                return;
            }

            _processedFeatures.Add(feature);

            _logger.LogInformation("Updating feature: {0}", feature);

            // proceed with dependent features first, whatever the module it's in
            var dependencies = _extensionManager.AvailableFeatures()
                .Where(f => String.Equals(f.Id, feature, StringComparison.OrdinalIgnoreCase))
                .Where(f => f.Dependencies != null)
                .SelectMany(f => f.Dependencies)
                .ToList();

            foreach (var dependency in dependencies)
            {
                await UpdateAsync(dependency);
            }

            var migrations = GetDataMigrations(feature);

            // apply update methods to each migration class for the module
            foreach (var migration in migrations)
            {
                // Create a new transaction for this migration
                //await _session.CommitAsync();

                await _store.ExecuteMigrationAsync(async schemaBuilder =>
                {
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
                            try
                            {
                                _logger.LogInformation("Applying migration for {0} from version {1}.", feature, current);
                                current = (int)lookupTable[current].Invoke(migration, new object[0]);
                            }
                            catch (Exception ex)
                            {
                                if (ex.IsFatal())
                                {
                                    throw;
                                }
                                _logger.LogError(0, "An unexpected error occurred while applying migration on {0} from version {1}.", feature, current);
                                throw;
                            }
                        }

                        // if current is 0, it means no upgrade/create method was found or succeeded
                        if (current == 0)
                        {
                            return;
                        }
                        if (dataMigrationRecord == null)
                        {
                            _dataMigrationRecord.DataMigrations.Add(new DataMigration { Version = current, DataMigrationClass = migration.GetType().FullName });
                        }
                        else
                        {
                            dataMigrationRecord.Version = current;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.IsFatal())
                        {
                            throw;
                        }
                        _logger.LogError(0, "Error while running migration version {0} for {1}.", current, feature);
                        _session.Cancel();
                        throw new OrchardException(T("Error while running migration version {0} for {1}.", current, feature), ex);
                    }
                });
            }
        }

        private async Task<DataMigration> GetDataMigrationRecordAsync(IDataMigration tempMigration)
        {
            return (await GetDataMigrationRecord()).DataMigrations
                .FirstOrDefault(dm => dm.DataMigrationClass == tempMigration.GetType().FullName);
        }

        /// <summary>
        /// Returns all the available IDataMigration instances for a specific module, and inject necessary builders
        /// </summary>
        private IEnumerable<IDataMigration> GetDataMigrations(string feature)
        {
            var migrations = _dataMigrations
                    .Where(dm => String.Equals(_typeFeatureProvider.GetFeatureForDependency(dm.GetType()).Descriptor.Id, feature, StringComparison.OrdinalIgnoreCase))
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
    }
}