using System.Text.Json.Nodes;

namespace OrchardCore.Deployment;

/// <summary>Explicit patch contract for a deployment step with one Boolean option.</summary>
/// <typeparam name="TStep">The exact deployment step type registered by the module.</typeparam>
public sealed class BooleanDeploymentStepDefinition<TStep> : IDeploymentStepDefinition where TStep : DeploymentStep
{
    private readonly string _property;
    private readonly Func<TStep, bool> _read;
    private readonly Action<TStep, bool> _write;

    /// <summary>Creates a contract from an allowlisted property and strongly typed accessors.</summary>
    public BooleanDeploymentStepDefinition(string type, string property, Func<TStep, bool> read, Action<TStep, bool> write)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(property);
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(write);
        Type = type; _property = property; _read = read; _write = write;
    }

    /// <inheritdoc />
    public string Type { get; }

    /// <inheritdoc />
    public JsonObject GetSchema() => new()
    {
        ["type"] = "object", ["additionalProperties"] = false,
        ["properties"] = new JsonObject { [_property] = new JsonObject { ["type"] = "boolean" } },
    };

    /// <inheritdoc />
    public JsonObject Describe(DeploymentStep step) => new() { [_property] = _read(GetStep(step)) };

    /// <inheritdoc />
    public ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        var target = GetStep(step);
        var errors = new Dictionary<string, string[]>();
        if (values is null || values.Any(value => value.Key != _property))
        {
            errors["values"] = ["Provide an object containing only the advertised option."];
        }
        else if (values.TryGetPropertyValue(_property, out var value))
        {
            if (value is JsonValue scalar && scalar.TryGetValue<bool>(out var option)) { _write(target, option); }
            else { errors[_property] = ["Provide a Boolean value."]; }
        }
        return ValueTask.FromResult<IReadOnlyDictionary<string, string[]>>(errors);
    }

    private static TStep GetStep(DeploymentStep step) => step?.GetType() == typeof(TStep) ? (TStep)step
        : throw new ArgumentException("The deployment step does not match this contract.", nameof(step));
}
