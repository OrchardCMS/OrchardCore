using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Activities;

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

    /// <summary>
    /// The values the activity provides to the activities that run after it, declared with its registration (see
    /// <see cref="IActivityProvidedValues"/>). Values the activity declares itself replace those of the same source
    /// and name.
    /// </summary>
    public IList<ActivityProvidedValue> ProvidedValues { get; } = [];

    /// <summary>
    /// Declares a value the activity provides, for example
    /// <c>activity.Provides(WorkflowValueSource.Input, "Owner", "any", "The owner of the content item.")</c>.
    /// </summary>
    /// <param name="source">Where the value is.</param>
    /// <param name="name">The key of the value in its source.</param>
    /// <param name="typeName">The variable type name of the value, or <c>any</c>.</param>
    /// <param name="description">What the value is, shown in the designer.</param>
    /// <param name="members">The fields of the value, for example <c>WorkflowValueMembers.User(S)</c>.</param>
    public ActivityRegistration Provides(
        WorkflowValueSource source,
        string name,
        string typeName = "any",
        string description = null,
        IEnumerable<ActivityProvidedValueMember> members = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        ProvidedValues.Add(new ActivityProvidedValue
        {
            Source = source,
            Name = name,
            TypeName = string.IsNullOrEmpty(typeName) ? "any" : typeName,
            Description = new LocalizedString(name, description ?? string.Empty),
            Members = members?.ToList() ?? [],
        });

        return this;
    }
}
