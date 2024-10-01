using System.Reflection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration.Records;
using OrchardCore.Environment.Extensions;
using OrchardCore.Modules;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Data.Migration;

/// <summary>
/// Represents a class that manages the database migrations.
/// </summary>
public class DataMigrationManager : IDataMigrationManager
{
    private const string _updateFromPrefix = "UpdateFrom";
    private const string _asyncSuffix = "Async";

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
        _processedFeatures = [];
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
                await _session.SaveAsync(_dataMigrationRecord);
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

            return GetMethod(dataMigration, "Create") != null;
        });

        return outOfDateMigrations.Select(m => _typeFeatureProvider.GetFeatureForDependency(m.GetType()).Id).ToArray();
    }

    public async Task Uninstall(string feature)
    {
        _logger.LogInformation("Uninstalling feature '{FeatureName}'.", feature);

        var migrations = GetDataMigrations(feature);

        // apply update methods to each migration class for the module
        foreach (var migration in migrations)
        {
            // copy the object for the Linq query
            var tempMigration = migration;

            // get current version for this migration
            var dataMigrationRecord = await GetDataMigrationRecordAsync(tempMigration);

            var uninstallMethod = GetMethod(migration, "Uninstall");

            if (uninstallMethod != null)
            {
                await InvokeMethodAsync(uninstallMethod, migration);
            }

            if (dataMigrationRecord == null)
            {
                continue;
            }

            (await GetDataMigrationRecordAsync()).DataMigrations.Remove(dataMigrationRecord);
        }
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

    public async Task UpdateAsync(params string[] featureIds)
    {
        foreach (var featureId in featureIds)
        {
            if (!_processedFeatures.Contains(featureId))
            {
                await UpdateAsync(featureId);
            }
        }
    }

    private async Task UpdateAsync(string featureId)
    {
        if (_processedFeatures.Contains(featureId))
        {
            return;
        }

        _processedFeatures.Add(featureId);

        _logger.LogInformation("Updating feature '{FeatureName}'", featureId);

        // proceed with dependent features first, whatever the module it's in
        var dependencies = _extensionManager
            .GetFeatureDependencies(featureId)
            .Where(x => x.Id != featureId)
            .Select(x => x.Id);

        foreach (var dependency in dependencies)
        {
            await UpdateAsync(dependency);
        }

        var migrations = GetDataMigrations(featureId);

        // apply update methods to each migration class for the module
        foreach (var migration in migrations)
        {
            var schemaBuilder = new SchemaBuilder(_store.Configuration, await _session.BeginTransactionAsync());
            migration.SchemaBuilder = schemaBuilder;

            // Copy the object for the Linq query.
            var tempMigration = migration;

            // Get current version for this migration.
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
                // Do we need to call Create or CreateAsync?
                if (current == 0)
                {
                    // Try to get a Create method.
                    var createMethod = GetMethod(migration, "Create");

                    if (createMethod == null)
                    {
                        _logger.LogWarning("The migration '{Name}' for '{FeatureName}' does not contain a proper Create or CreateAsync method.", migration.GetType().FullName, featureId);
                        continue;
                    }

                    current = await InvokeMethodAsync(createMethod, migration);
                }

                var lookupTable = CreateUpgradeLookupTable(migration);

                while (lookupTable.TryGetValue(current, out var methodInfo))
                {
                    _logger.LogInformation("Applying migration for '{FeatureName}' from version {Version}.", featureId, current);

                    current = await InvokeMethodAsync(methodInfo, migration);
                }

                // If current is 0, it means no upgrade/create method was found or succeeded.
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
                // Persist data migrations.
                await _session.SaveAsync(_dataMigrationRecord);
            }
        }
    }

    private static async Task<int> InvokeMethodAsync(MethodInfo method, IDataMigration migration)
    {
        if (method.ReturnType == typeof(Task<int>))
        {
            return await (Task<int>)method.Invoke(migration, []);
        }

        if (method.ReturnType == typeof(int))
        {
            return (int)method.Invoke(migration, []);
        }

        throw new InvalidOperationException("Invalid return type used in a migration method.");
    }

    private async Task<Records.DataMigration> GetDataMigrationRecordAsync(IDataMigration tempMigration)
    {
        var dataMigrationRecord = await GetDataMigrationRecordAsync();
        return dataMigrationRecord
            .DataMigrations
            .FirstOrDefault(dm => dm.DataMigrationClass == tempMigration.GetType().FullName);
    }

    /// <summary>
    /// Returns all the available IDataMigration instances for a specific module, and inject necessary builders.
    /// </summary>
    private IDataMigration[] GetDataMigrations(string featureId)
    {
        var migrations = _dataMigrations
                .Where(dm => _typeFeatureProvider.GetFeatureForDependency(dm.GetType()).Id == featureId)
                .ToArray();

        return migrations;
    }

    /// <summary>
    /// Create a list of all available Update methods from a data migration class, indexed by the version number.
    /// </summary>
    private static Dictionary<int, MethodInfo> CreateUpgradeLookupTable(IDataMigration dataMigration)
        => dataMigration
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(GetUpdateFromMethod)
            .Where(tuple => tuple != null)
            .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

    private static Tuple<int, MethodInfo> GetUpdateFromMethod(MethodInfo methodInfo)
    {
        if (methodInfo.Name.StartsWith(_updateFromPrefix, StringComparison.Ordinal) &&
            (methodInfo.ReturnType == typeof(int) || methodInfo.ReturnType == typeof(Task<int>)))
        {
            var version = methodInfo.Name.EndsWith(_asyncSuffix, StringComparison.Ordinal)
                ? methodInfo.Name.Substring(_updateFromPrefix.Length, methodInfo.Name.Length - _updateFromPrefix.Length - _asyncSuffix.Length)
                : methodInfo.Name[_updateFromPrefix.Length..];

            if (int.TryParse(version, out var versionValue))
            {
                return new Tuple<int, MethodInfo>(versionValue, methodInfo);
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the method from a data migration class that matches the given name if found.
    /// </summary>
    private static MethodInfo GetMethod(IDataMigration dataMigration, string name)
    {
        // First try to find a method that match the given name. (Ex. Create())
        var methodInfo = dataMigration.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Instance);

        if (methodInfo != null && (methodInfo.ReturnType == typeof(int) || methodInfo.ReturnType == typeof(Task<int>)))
        {
            return methodInfo;
        }

        // At this point, try to find a method that matches the given name and ends with Async. (Ex. CreateAsync())
        methodInfo = dataMigration.GetType().GetMethod(name + _asyncSuffix, BindingFlags.Public | BindingFlags.Instance);

        if (methodInfo != null && methodInfo.ReturnType == typeof(Task<int>))
        {
            return methodInfo;
        }

        return null;
    }
}
