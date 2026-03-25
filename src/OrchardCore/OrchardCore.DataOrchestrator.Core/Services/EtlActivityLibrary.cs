using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.DataOrchestrator.Activities;

namespace OrchardCore.DataOrchestrator.Services;

/// <summary>
/// Default implementation of <see cref="IEtlActivityLibrary"/> that manages
/// the registry of available ETL activities.
/// </summary>
public sealed class EtlActivityLibrary : IEtlActivityLibrary
{
    private readonly Lazy<IDictionary<string, IEtlActivity>> _activityDictionary;
    private readonly Lazy<IList<string>> _activityCategories;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public EtlActivityLibrary(
        IOptions<EtlOptions> etlOptions,
        IServiceProvider serviceProvider,
        ILogger<EtlActivityLibrary> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _activityDictionary = new Lazy<IDictionary<string, IEtlActivity>>(() =>
            etlOptions.Value.ActivityTypes
                .Where(x => !x.IsAbstract)
                .Select(x => serviceProvider.CreateInstance<IEtlActivity>(x))
                .OrderBy(x => x.Name)
                .ToDictionary(x => x.Name));

        _activityCategories = new Lazy<IList<string>>(() =>
            _activityDictionary.Value.Values
                .OrderBy(x => x.Category)
                .Select(x => x.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList());
    }

    private IDictionary<string, IEtlActivity> ActivityDictionary => _activityDictionary.Value;

    private IList<string> ActivityCategories => _activityCategories.Value;

    /// <inheritdoc />
    public IEnumerable<IEtlActivity> ListActivities()
    {
        return ActivityDictionary.Values;
    }

    /// <inheritdoc />
    public IEnumerable<string> ListCategories()
    {
        return ActivityCategories;
    }

    /// <inheritdoc />
    public IEtlActivity GetActivityByName(string name)
    {
        return ActivityDictionary.TryGetValue(name, out var activity) ? activity : null;
    }

    /// <inheritdoc />
    public IEtlActivity InstantiateActivity(string name)
    {
        var activityType = GetActivityByName(name)?.GetType();

        if (activityType == null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Requested ETL activity '{ActivityName}' does not exist in the library.", name);
            }

            return null;
        }

        return _serviceProvider.CreateInstance<IEtlActivity>(activityType);
    }
}
