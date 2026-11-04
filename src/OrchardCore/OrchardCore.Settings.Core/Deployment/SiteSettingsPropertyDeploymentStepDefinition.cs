using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Settings.Deployment;

internal sealed class SiteSettingsPropertyDeploymentStepDefinition<TModel> : IDeploymentStepDefinition where TModel : class, new()
{
    public string Type { get; } = new SiteSettingsPropertyDeploymentStepFactory<TModel>().Name;
    public JsonObject GetSchema() => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema", ["type"] = "object",
        ["additionalProperties"] = false, ["properties"] = new JsonObject(),
        ["description"] = "Includes this tenant settings section in an export; it has no step configuration and does not expose settings values.",
    };
    public JsonObject Describe(DeploymentStep step) => step is SiteSettingsPropertyDeploymentStep<TModel>
        ? new() : throw new ArgumentException("The step does not match this settings factory.", nameof(step));
    public ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        IReadOnlyDictionary<string, string[]> errors = step is not SiteSettingsPropertyDeploymentStep<TModel> || values is null || values.Count != 0
            ? new Dictionary<string, string[]> { ["values"] = ["This settings export step requires an empty configuration object."] }
            : new Dictionary<string, string[]>();
        return ValueTask.FromResult(errors);
    }
}
