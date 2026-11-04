namespace OrchardCore.Deployment;

/// <summary>Reads and maintains tenant deployment plans for admin, recipes and remote callers.</summary>
public interface IDeploymentPlanService
{
    /// <summary>Gets a tracked plan by its tenant-local identifier.</summary>
    Task<DeploymentPlan> GetAsync(long id);
    /// <summary>Gets a tracked plan using the store's name comparison.</summary>
    Task<DeploymentPlan> FindByNameAsync(string name);
    /// <summary>Lists plans using the existing admin search and stable name/identifier ordering.</summary>
    Task<DeploymentPlanPage> ListAsync(string search, int skip, int take);
    /// <summary>Creates an empty plan, rejecting an empty or already used name.</summary>
    Task<DeploymentPlanManagementResult> CreateAsync(string name);
    /// <summary>Renames a plan without changing its steps; equivalent names do not save.</summary>
    Task<DeploymentPlanManagementResult> RenameAsync(long id, string name);
    /// <summary>Deletes a plan, returning false if it was already absent.</summary>
    Task<bool> DeleteAsync(long id);

    /// <summary>Creates a detached editor copy using the tenant's deployment serialization contracts.</summary>
    DeploymentStep CloneStep(DeploymentStep step);
    /// <summary>Compares persisted step configuration using the tenant document contract.</summary>
    bool StepEquals(DeploymentStep left, DeploymentStep right);
    /// <summary>Appends steps after validating all identities; missing identities are generated.</summary>
    Task<DeploymentStepManagementResult> AddStepsAsync(long id, IEnumerable<DeploymentStep> steps);
    /// <summary>Replaces a step with a validated editor candidate of the same type and identity.</summary>
    Task<DeploymentStepManagementResult> UpdateStepAsync(long id, DeploymentStep step);
    /// <summary>Removes the identified step without changing the remaining order.</summary>
    Task<DeploymentStepManagementResult> DeleteStepAsync(long id, string stepId);
    /// <summary>Moves an admin-selected step after validating both zero-based positions.</summary>
    Task<DeploymentStepManagementResult> MoveStepAsync(long id, int oldIndex, int newIndex);
    /// <summary>Applies a complete, unique list of existing step identities as the new order.</summary>
    Task<DeploymentStepManagementResult> ReorderStepsAsync(long id, IReadOnlyList<string> stepIds);

    Task<bool> DoesUserHavePermissionsAsync();
    Task<bool> DoesUserHaveExportPermissionAsync();
    Task<IEnumerable<string>> GetAllDeploymentPlanNamesAsync();
    Task<IEnumerable<DeploymentPlan>> GetAllDeploymentPlansAsync();
    Task<IEnumerable<DeploymentPlan>> GetDeploymentPlansAsync(params string[] deploymentPlanNames);
    /// <summary>Validates the complete replacement batch without changing any plan.</summary>
    IReadOnlyDictionary<string, string[]> ValidateReplacement(IReadOnlyList<DeploymentPlan> deploymentPlans);
    /// <summary>Validates then creates or replaces all plans; invalid input throws before mutation.</summary>
    Task CreateOrUpdateDeploymentPlansAsync(IEnumerable<DeploymentPlan> deploymentPlans);
}
