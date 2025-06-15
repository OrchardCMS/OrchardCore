namespace OrchardCore.Deployment;

/// <summary>
/// The state of a deployment plan built by sources.
/// </summary>
public class DeploymentPlan
{
    public long Id { get; set; }
    public string Name { get; set; }
#pragma warning disable MA0016 // Prefer using collection abstraction instead of implementation
    public List<DeploymentStep> DeploymentSteps { get; init; } = [];
#pragma warning restore MA0016 // Prefer using collection abstraction instead of implementation
}
