using System.Text.Json.Nodes;

namespace OrchardCore.Deployment;

/// <summary>Explicit configuration contract for one registered deployment step factory.</summary>
public interface IDeploymentStepDefinition
{
    /// <summary>Gets the factory name, including any generic settings discriminator.</summary>
    string Type { get; }
    /// <summary>Gets the patch schema; omitted fields retain their values.</summary>
    JsonObject GetSchema();
    /// <summary>Describes allowlisted configuration, omitting write-only fields.</summary>
    JsonObject Describe(DeploymentStep step);
    /// <summary>Applies a patch to a detached candidate and validates it before persistence.</summary>
    ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values);
}
