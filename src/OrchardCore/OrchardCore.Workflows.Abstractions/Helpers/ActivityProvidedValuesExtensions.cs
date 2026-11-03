using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Workflows.Helpers;

public static class ActivityProvidedValuesExtensions
{
    /// <summary>
    /// Returns the values an activity provides: those declared with its registration, and those it declares itself
    /// (<see cref="IActivityProvidedValues"/>), which replace the registration's of the same source and name.
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValue> GetProvidedValues(this IActivity activity, WorkflowOptions options)
    {
        ArgumentNullException.ThrowIfNull(activity);

        var values = new List<ActivityProvidedValue>();

        if (options?.GetActivityRegistration(activity.GetType()) is { } registration)
        {
            values.AddRange(registration.ProvidedValues);
        }

        if (activity is IActivityProvidedValues provider)
        {
            foreach (var value in provider.GetProvidedValues() ?? [])
            {
                if (value is null || string.IsNullOrEmpty(value.Name))
                {
                    continue;
                }

                values.RemoveAll(existing => existing.Source == value.Source && string.Equals(existing.Name, value.Name, StringComparison.Ordinal));
                values.Add(value);
            }
        }

        return values;
    }
}
