using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Deployment;

/// <summary>
/// A concrete placeholder for deployment steps whose original type is no longer registered,
/// typically because the feature that provides it has been disabled. Preserves the original
/// JSON data for round-trip serialization and prevents deserialization crashes.
/// </summary>
public sealed class UnknownDeploymentStep : DeploymentStep, IUnknownTypePlaceholder
{
    public string TypeDiscriminator { get; set; }

    public JsonElement RawData { get; set; }
}
