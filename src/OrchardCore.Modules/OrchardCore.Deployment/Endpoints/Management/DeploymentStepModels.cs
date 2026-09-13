using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.Deployment.Endpoints.Management;

/// <summary>Creates a step with a caller-selected identity for safe retries.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DeploymentStepCreateRequest
{
    /// <summary>Gets a nonempty, caller-selected identity of at most 128 characters.</summary>
    [Required, MaxLength(128)]
    public string Id { get; init; }
    /// <summary>Gets the registered factory name.</summary>
    [Required]
    public string Type { get; init; }
    /// <summary>Gets configuration matching the factory's explicit schema.</summary>
    [Required]
    public JsonObject Values { get; init; }
}

/// <summary>Patches a step without changing its identity or type.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DeploymentStepUpdateRequest
{
    /// <summary>Gets the configuration patch; omitted fields retain their values.</summary>
    [Required]
    public JsonObject Values { get; init; }
}

/// <summary>Supplies every current step identity exactly once in the desired order.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DeploymentStepOrderRequest
{
    /// <summary>Gets the complete desired order.</summary>
    [Required]
    public string[] StepIds { get; init; }
}

/// <summary>Describes step identity and explicitly readable configuration.</summary>
public sealed class DeploymentStepResponse
{
    /// <summary>Gets the stored identity; old plans may contain missing identities until migrated.</summary>
    public string Id { get; init; }
    /// <summary>Gets the factory identity used by recipes.</summary>
    public string Type { get; init; }
    /// <summary>Gets the zero-based position in the plan.</summary>
    public int Position { get; init; }
    /// <summary>Gets whether an enabled explicit contract can configure this step.</summary>
    public bool CanConfigure { get; init; }
    /// <summary>Gets allowlisted values, omitting write-only fields; null for unsupported steps.</summary>
    public JsonObject Values { get; init; }
}

/// <summary>Reports whether a step mutation changed the plan.</summary>
public sealed class DeploymentStepWriteResponse
{
    /// <summary>Gets whether persisted state changed.</summary>
    public bool Changed { get; init; }
    /// <summary>Gets the resulting step, or null after deleting or reordering.</summary>
    public DeploymentStepResponse Step { get; init; }
}
