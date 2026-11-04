using System.Text.Json.Nodes;

namespace OrchardCore.Deployment;

/// <summary>Explicitly opts an audited, feature-owned step into management without configurable properties.</summary>
/// <typeparam name="TStep">The exact registered deployment step type.</typeparam>
public sealed class EmptyDeploymentStepDefinition<TStep> : IDeploymentStepDefinition where TStep : DeploymentStep
{
    private readonly string _description;

    /// <summary>Creates a contract for the named factory with an optional description of its export behavior.</summary>
    public EmptyDeploymentStepDefinition(string type, string description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        Type = type;
        _description = description;
    }

    /// <inheritdoc />
    public string Type { get; }

    /// <inheritdoc />
    public JsonObject GetSchema()
    {
        var schema = new JsonObject { ["type"] = "object", ["additionalProperties"] = false, ["properties"] = new JsonObject() };
        if (_description is not null) { schema["description"] = _description; }
        return schema;
    }

    /// <inheritdoc />
    public JsonObject Describe(DeploymentStep step)
    {
        ValidateType(step);
        return [];
    }

    /// <inheritdoc />
    public ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        ValidateType(step);
        return ValueTask.FromResult<IReadOnlyDictionary<string, string[]>>(values is { Count: 0 }
            ? new Dictionary<string, string[]>() : new Dictionary<string, string[]> { ["values"] = ["This step accepts an empty configuration object only."] });
    }

    private static void ValidateType(DeploymentStep step)
    {
        if (step?.GetType() != typeof(TStep))
        {
            throw new ArgumentException("The step does not match this configuration contract.", nameof(step));
        }
    }
}
