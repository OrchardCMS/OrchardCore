namespace OrchardCore.Deployment;

/// <summary>Describes why an administrative plan mutation was rejected.</summary>
public enum DeploymentPlanManagementError
{
    /// <summary>The operation was accepted.</summary>
    None,
    /// <summary>A nonempty plan name is required.</summary>
    MissingName,
    /// <summary>Another plan uses the requested name.</summary>
    DuplicateName,
    /// <summary>The requested plan does not exist in this tenant.</summary>
    NotFound,
}

/// <summary>The outcome of creating or renaming a plan without changing its steps.</summary>
public sealed class DeploymentPlanManagementResult
{
    /// <summary>Gets the accepted plan, or null when the operation was rejected.</summary>
    public DeploymentPlan Plan { get; init; }
    /// <summary>Gets whether persisted state changed.</summary>
    public bool Changed { get; init; }
    /// <summary>Gets the rejection reason, if any.</summary>
    public DeploymentPlanManagementError Error { get; init; }
}

/// <summary>A page of tenant deployment plans in name and identifier order.</summary>
public sealed class DeploymentPlanPage
{
    /// <summary>Gets the matching count before paging.</summary>
    public int TotalCount { get; init; }
    /// <summary>Gets the plans on the requested page.</summary>
    public IReadOnlyList<DeploymentPlan> Items { get; init; } = [];
}
