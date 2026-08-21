using OrchardCore.DataOrchestrator.Activities;

namespace OrchardCore.DataOrchestrator;

/// <summary>
/// Configuration options for registered ETL activities and their display drivers.
/// </summary>
public sealed class EtlOptions
{
    private Dictionary<Type, EtlActivityRegistration> ActivityDictionary { get; } = [];

    /// <summary>
    /// Gets the registered activity types.
    /// </summary>
    public IEnumerable<Type> ActivityTypes => ActivityDictionary.Values.Select(x => x.ActivityType).ToList().AsReadOnly();

    /// <summary>
    /// Gets the display driver types for all registered activities.
    /// </summary>
    public IEnumerable<Type> ActivityDisplayDriverTypes => ActivityDictionary.Values.SelectMany(x => x.DriverTypes).ToList().AsReadOnly();

    /// <summary>
    /// Registers an activity type and optionally a display driver type.
    /// </summary>
    public EtlOptions RegisterActivity(Type activityType, Type driverType = null)
    {
        if (ActivityDictionary.TryGetValue(activityType, out var value))
        {
            if (driverType != null)
            {
                value.DriverTypes.Add(driverType);
            }
        }
        else
        {
            ActivityDictionary.Add(activityType, new EtlActivityRegistration(activityType, driverType));
        }

        return this;
    }

    /// <summary>
    /// Checks whether an activity type is registered.
    /// </summary>
    public bool IsActivityRegistered(Type activityType)
    {
        return ActivityDictionary.ContainsKey(activityType);
    }
}

/// <summary>
/// Represents a registered ETL activity with its display driver types.
/// </summary>
public sealed class EtlActivityRegistration
{
    public EtlActivityRegistration(Type activityType, Type driverType)
    {
        ActivityType = activityType;

        if (driverType != null)
        {
            DriverTypes.Add(driverType);
        }
    }

    public Type ActivityType { get; }

    public IList<Type> DriverTypes { get; } = [];
}

public static class EtlOptionsExtensions
{
    public static EtlOptions RegisterActivity<TActivity, TDriver>(this EtlOptions options)
        where TActivity : IEtlActivity
    {
        return options.RegisterActivity(typeof(TActivity), typeof(TDriver));
    }
}
