namespace OrchardCore.Deployment.ViewModels;

/// <summary>
/// The deployment plan edited by the edit page: the fields of the create page, and the identifier of the plan.
/// </summary>
public class EditDeploymentPlanViewModel : CreateDeploymentPlanViewModel
{
    public long Id { get; set; }
}
