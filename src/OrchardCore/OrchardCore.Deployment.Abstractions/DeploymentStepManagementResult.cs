namespace OrchardCore.Deployment;

/// <summary>Describes a rejected step mutation.</summary>
public enum DeploymentStepManagementError
{
    /// <summary>The operation was accepted.</summary>
    None,
    /// <summary>The plan or step does not exist.</summary>
    NotFound,
    /// <summary>The supplied step is missing, duplicates an identifier or changes its type.</summary>
    InvalidStep,
    /// <summary>The requested order does not identify a valid complete ordering.</summary>
    InvalidOrder,
}

/// <summary>The outcome of a shared deployment step mutation.</summary>
public sealed class DeploymentStepManagementResult
{
    /// <summary>Gets whether stored state changed.</summary>
    public bool Changed { get; init; }
    /// <summary>Gets the rejection reason, if any.</summary>
    public DeploymentStepManagementError Error { get; init; }
}
