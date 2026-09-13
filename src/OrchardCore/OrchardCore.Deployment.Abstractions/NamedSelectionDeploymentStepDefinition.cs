using System.Text.Json.Nodes;

namespace OrchardCore.Deployment;

/// <summary>Explicit contract for a deployment step that selects named entries or includes all entries.</summary>
/// <typeparam name="TStep">The exact feature-owned deployment step type.</typeparam>
public sealed class NamedSelectionDeploymentStepDefinition<TStep> : IDeploymentStepDefinition where TStep : DeploymentStep
{
    private readonly string _property;
    private readonly Func<Task<IEnumerable<string>>> _availableNames;
    private readonly Func<TStep, (bool IncludeAll, string[] Names)> _read;
    private readonly Action<TStep, bool, string[]> _write;

    /// <summary>Creates a contract with explicit property access and the owning service's available names.</summary>
    public NamedSelectionDeploymentStepDefinition(string type, string property, Func<Task<IEnumerable<string>>> availableNames,
        Func<TStep, (bool IncludeAll, string[] Names)> read, Action<TStep, bool, string[]> write)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(property);
        ArgumentNullException.ThrowIfNull(availableNames);
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(write);
        Type = type;
        _property = property;
        _availableNames = availableNames;
        _read = read;
        _write = write;
    }

    /// <inheritdoc />
    public string Type { get; }

    /// <inheritdoc />
    public JsonObject GetSchema() => new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["includeAll"] = new JsonObject { ["type"] = "boolean" },
            [_property] = new JsonObject { ["type"] = "array", ["items"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 } },
        },
    };

    /// <inheritdoc />
    public JsonObject Describe(DeploymentStep step)
    {
        var (includeAll, names) = _read(GetStep(step));
        return new JsonObject
        {
            ["includeAll"] = includeAll,
            [_property] = new JsonArray((names ?? []).Select(name => (JsonNode)JsonValue.Create(name)).ToArray()),
        };
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        var target = GetStep(step);
        var (includeAll, names) = _read(target);
        var errors = new Dictionary<string, string[]>();
        if (values is null || values.Any(value => value.Key != "includeAll" && value.Key != _property))
        {
            errors["values"] = ["Provide only properties from the deployment step schema."];
            return errors;
        }

        foreach (var (property, value) in values)
        {
            if (property == "includeAll")
            {
                if (value is JsonValue scalar && scalar.TryGetValue<bool>(out var flag))
                {
                    includeAll = flag;
                }
                else
                {
                    errors[property] = ["Provide a Boolean value."];
                }
            }
            else if (value is JsonArray array && array.All(item => item is JsonValue scalar
                && scalar.TryGetValue<string>(out var name) && !string.IsNullOrWhiteSpace(name)))
            {
                names = array.Select(item => item.GetValue<string>()).ToArray();
            }
            else
            {
                errors[property] = ["Provide an array of non-empty names."];
            }
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        names = DeploymentSelection.Normalize(includeAll, names);
        if (!includeAll)
        {
            var available = new HashSet<string>(await _availableNames(), StringComparer.Ordinal);
            if (names.Any(name => !available.Contains(name)))
            {
                errors[_property] = ["Select existing names."];
            }
        }

        if (errors.Count == 0)
        {
            _write(target, includeAll, names);
        }

        return errors;
    }

    private static TStep GetStep(DeploymentStep step) => step?.GetType() == typeof(TStep) ? (TStep)step
        : throw new ArgumentException("The step does not match this configuration contract.", nameof(step));
}
