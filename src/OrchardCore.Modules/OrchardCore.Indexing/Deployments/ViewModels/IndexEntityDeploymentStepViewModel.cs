namespace OrchardCore.Indexing.Deployments.ViewModels;

public class IndexEntityDeploymentStepViewModel
{
    public bool IncludeAll { get; set; }

    public IndexEntitySourceViewModel[] Indexes { get; set; }
}
