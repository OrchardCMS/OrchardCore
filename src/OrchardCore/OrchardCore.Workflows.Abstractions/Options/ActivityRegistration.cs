namespace OrchardCore.Workflows.Options;

/// <summary>
/// The registration of an activity type with its display drivers.
/// </summary>
public class ActivityRegistration
{
    public ActivityRegistration(Type activityType)
    {
        ActivityType = activityType;
        DriverTypes = [];
    }

    public ActivityRegistration(Type activityType, Type driverType) : this(activityType)
    {
        DriverTypes.Add(driverType);
    }

    /// <summary>
    /// The activity type.
    /// </summary>
    public Type ActivityType { get; }

    /// <summary>
    /// The display drivers of the activity.
    /// </summary>
    public HashSet<Type> DriverTypes { get; }

    /// <summary>
    /// An optional Font Awesome icon class shown by the workflow designer, for example <c>fa-solid fa-bell</c>.
    /// When it isn't set, the designer uses a default icon for the activity's category.
    /// </summary>
    public string Icon { get; set; }
}
