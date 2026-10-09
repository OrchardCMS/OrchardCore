using System.Text.Json.Nodes;

namespace OrchardCore.Workflows.Helpers;

/// <summary>
/// Reads and writes the output bindings of an activity: which workflow variable each output is written to. They
/// are stored in the activity's properties, so they are versioned with it.
/// </summary>
public static class ActivityOutputBindingExtensions
{
    /// <summary>
    /// The activity property that holds the bindings, an object of output names to variable names.
    /// </summary>
    public const string PropertyName = "OutputBindings";

    /// <summary>
    /// Returns the bindings of an activity, by output name.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetOutputBindings(this JsonObject properties)
    {
        var bindings = new Dictionary<string, string>(StringComparer.Ordinal);

        if (properties?[PropertyName] is JsonObject stored)
        {
            foreach (var (output, variable) in stored)
            {
                if (variable is JsonValue value && value.TryGetValue<string>(out var name) && !string.IsNullOrWhiteSpace(name))
                {
                    bindings[output] = name;
                }
            }
        }

        return bindings;
    }

    /// <summary>
    /// Replaces the bindings of an activity. Empty variable names are left out, and no bindings remove the property.
    /// </summary>
    public static void SetOutputBindings(this JsonObject properties, IDictionary<string, string> bindings)
    {
        ArgumentNullException.ThrowIfNull(properties);

        var stored = new JsonObject();

        foreach (var (output, variable) in bindings ?? new Dictionary<string, string>())
        {
            if (!string.IsNullOrWhiteSpace(output) && !string.IsNullOrWhiteSpace(variable))
            {
                stored[output] = variable;
            }
        }

        if (stored.Count == 0)
        {
            properties.Remove(PropertyName);
        }
        else
        {
            properties[PropertyName] = stored;
        }
    }
}
