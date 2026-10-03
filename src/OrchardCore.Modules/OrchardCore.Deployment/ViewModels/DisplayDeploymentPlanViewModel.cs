namespace OrchardCore.Deployment.ViewModels;

public class DisplayDeploymentPlanViewModel
{
    public DeploymentPlan DeploymentPlan { get; set; }

    public IEnumerable<DisplayDeploymentPlanCategoryViewModel> Categories { get; set; }

    public IEnumerable<dynamic> Items { get; set; }

    public IEnumerable<DisplayDeploymentPlanThumbnailViewModel> Thumbnails { get; set; }
}
