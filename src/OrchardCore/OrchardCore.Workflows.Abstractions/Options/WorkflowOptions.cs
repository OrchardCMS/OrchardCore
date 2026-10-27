namespace OrchardCore.Workflows.Options;

public class WorkflowOptions
{
    public WorkflowOptions()
    {
        ActivityDictionary = [];
    }

    /// <summary>
    /// The set of activities available to workflows.
    /// Modules can register and unregister activities.
    /// </summary>
    private Dictionary<Type, ActivityRegistration> ActivityDictionary { get; }

    public IEnumerable<Type> ActivityTypes => ActivityDictionary.Values.Select(x => x.ActivityType).ToList().AsReadOnly();
    public IEnumerable<Type> ActivityDisplayDriverTypes => ActivityDictionary.Values.SelectMany(x => x.DriverTypes).ToList().AsReadOnly();

    /// <summary>
    /// The Font Awesome icons the designer shows for the activities of a category, by the untranslated name of the
    /// category, the <see cref="Microsoft.Extensions.Localization.LocalizedString.Name"/> of
    /// <see cref="Activities.IActivity.Category"/>. An icon set on the activity's
    /// <see cref="ActivityRegistration.Icon"/> wins. Each module sets the icons of its own categories, for example
    /// <c>services.Configure&lt;WorkflowOptions&gt;(options =&gt; options.CategoryIcons["Messaging"] = "fa-solid fa-envelope")</c>.
    /// </summary>
    public IDictionary<string, string> CategoryIcons { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public WorkflowOptions RegisterActivity(Type activityType, Type driverType = null)
        => RegisterActivity(activityType, driverType, null);

    /// <summary>
    /// Registers an activity type and an optional display driver, then lets <paramref name="configure"/>
    /// change the registration, for example to set its <see cref="ActivityRegistration.Icon"/>.
    /// </summary>
    public WorkflowOptions RegisterActivity(Type activityType, Type driverType, Action<ActivityRegistration> configure)
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
            value = new ActivityRegistration(activityType, driverType);
            ActivityDictionary.Add(activityType, value);
        }

        configure?.Invoke(value);

        return this;
    }

    /// <summary>
    /// Returns the registration of an activity type, or <see langword="null"/> when it isn't registered.
    /// </summary>
    public ActivityRegistration GetActivityRegistration(Type activityType)
    {
        ArgumentNullException.ThrowIfNull(activityType);

        return ActivityDictionary.GetValueOrDefault(activityType);
    }

    public WorkflowOptions UnregisterActivityType(Type activityType)
    {
        if (!ActivityDictionary.ContainsKey(activityType))
        {
            throw new InvalidOperationException("The specified activity type is not registered.");
        }

        ActivityDictionary.Remove(activityType);
        return this;
    }

    public bool IsActivityRegistered(Type activityType)
    {
        return ActivityDictionary.ContainsKey(activityType);
    }
}

public static class WorkflowOptionsExtensions
{
    public static WorkflowOptions RegisterActivityType<T>(this WorkflowOptions options)
    {
        return options.RegisterActivity(typeof(T));
    }

    public static WorkflowOptions RegisterActivity<T, TDriver>(this WorkflowOptions options)
    {
        return options.RegisterActivity(typeof(T), typeof(TDriver));
    }

    public static WorkflowOptions UnregisterActivityType<T>(this WorkflowOptions options)
    {
        return options.UnregisterActivityType(typeof(T));
    }

    public static bool IsActivityRegistered<T>(this WorkflowOptions options)
    {
        return options.IsActivityRegistered(typeof(T));
    }
}
