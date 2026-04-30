using OrchardCore.DisplayManagement;

namespace OrchardCore.Deployment.ViewModels;

public class DisplayDeploymentPlanThumbnailViewModel
{
    public string Category { get; set; }

    public string CategoryId { get; set; }

    public IShape Thumbnail { get; set; }

    public string Type { get; set; }
}
