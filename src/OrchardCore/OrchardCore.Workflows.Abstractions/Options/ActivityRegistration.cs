namespace OrchardCore.Workflows.Options;

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

    public Type ActivityType { get; }
    public HashSet<Type> DriverTypes { get; }
}
