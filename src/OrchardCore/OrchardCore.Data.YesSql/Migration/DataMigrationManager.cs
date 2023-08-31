using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration.Records;
using OrchardCore.Environment.Extensions;
using OrchardCore.Modules;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Data.Migration
{
    /// <summary>
    /// Represents a class that manages the database migrations.
    /// </summary>
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

        /// <summary>
        /// Creates a new instance of the <see cref="DataMigrationManager"/>.
        /// </summary>
        /// <param name="typeFeatureProvider">The <see cref="ITypeFeatureProvider"/>.</param>
        /// <param name="dataMigrations">A list of <see cref="IDataMigration"/>.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="store">The <see cref="IStore"/>.</param>
        /// <param name="extensionManager">The <see cref="IExtensionManager"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public DataMigrationManager(
            ITypeFeatureProvider typeFeatureProvider,
            IEnumerable<IDataMigration> dataMigrations,
            ISession session,
            IStore store,
            IExtensionManager extensionManager,
            ILogger<DataMigrationManager> logger)
        {
            _typeFeatureProvider = typeFeatureProvider;
            _dataMigrations = dataMigrations;
            _session = session;
            _store = store;
            _extensionManager = extensionManager;
            _logger = logger;
            _processedFeatures = new List<string>();
        }

        /// <inheritdocs />
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

                return ((GetCreateMethod(dataMigration) ?? GetCreateAsyncMethod(dataMigration)) != null);
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
                uninstallMethod?.Invoke(migration, Array.Empty<object>());

                var uninstallAsyncMethod = GetUninstallAsyncMethod(migration);
                if (uninstallAsyncMethod != null)
                {
                    await (Task)uninstallAsyncMethod.Invoke(migration, Array.Empty<object>());
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
                .GetFeatureDependencies(
                    featureId)
                .Where(x => x.Id != featureId)
                .Select(x => x.Id);

            await UpdateAsync(dependencies);

            var migrations = GetDataMigrations(featureId);

            // apply update methods to each migration class for the module
            foreach (var migration in migrations)
            {
                var schemaBuilder = new SchemaBuilder(_store.Configuration, await _session.BeginTransactionAsync());
                migration.SchemaBuilder = schemaBuilder;

                // copy the object for the Linq query
                var tempMigration = migration;

                // get current version for this migration
                var dataMigrationRecord = await GetDataMigrationRecordAsync(tempMigration);

                var current = 0;
                if (dataMigrationRecord != null)
                {
                    // This can be null if a failed create migration has occurred and the data migration record was saved.
                    current = dataMigrationRecord.Version ?? current;
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
                            current = (int)createMethod.Invoke(migration, Array.Empty<object>());
                        }

                        // try to resolve a CreateAsync method

                        var createAsyncMethod = GetCreateAsyncMethod(migration);
                        if (createAsyncMethod != null)
                        {
                            current = await (Task<int>)createAsyncMethod.Invoke(migration, Array.Empty<object>());
                        }
                    }

                    var lookupTable = CreateUpgradeLookupTable(migration);

                    while (lookupTable.TryGetValue(current, out var methodInfo))
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Applying migration for '{FeatureName}' from version {Version}.", featureId, current);
                        }

                        var isAwaitable = methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;
                        if (isAwaitable)
                        {
                            current = await (Task<int>)methodInfo.Invoke(migration, Array.Empty<object>());
                        }
                        else
                        {
                            current = (int)methodInfo.Invoke(migration, Array.Empty<object>());
                        }
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

                    await _session.CancelAsync();
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

        private static Tuple<int, MethodInfo> GetUpdateMethod(MethodInfo methodInfo)
        {
            const string updateFromPrefix = "UpdateFrom";
            const string asyncSuffix = "Async";

            if (methodInfo.Name.StartsWith(updateFromPrefix, StringComparison.Ordinal) && (methodInfo.ReturnType == typeof(int) || methodInfo.ReturnType == typeof(Task<int>)))
            {
                var version = methodInfo.Name.EndsWith(asyncSuffix, StringComparison.Ordinal)
                    ? methodInfo.Name.Substring(updateFromPrefix.Length, methodInfo.Name.Length - updateFromPrefix.Length - asyncSuffix.Length)
                    : methodInfo.Name[updateFromPrefix.Length..];

                if (Int32.TryParse(version, out var versionValue))
                {
                    return new Tuple<int, MethodInfo>(versionValue, methodInfo);
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
        /// Returns the CreateAsync method from a data migration class if it's found
        /// </summary>
        private static MethodInfo GetCreateAsyncMethod(IDataMigration dataMigration)
        {
            var methodInfo = dataMigration.GetType().GetMethod("CreateAsync", BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo != null && methodInfo.ReturnType == typeof(Task<int>))
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

        /// <summary>
        /// Returns the UninstallAsync method from a data migration class if it's found
        /// </summary>
        private static MethodInfo GetUninstallAsyncMethod(IDataMigration dataMigration)
        {
            var methodInfo = dataMigration.GetType().GetMethod("UninstallAsync", BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo != null && methodInfo.ReturnType == typeof(Task))
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
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.LogError(ex, "Could not run migrations automatically on '{FeatureName}'", featureId);
                }
            }
        }
    }
}
