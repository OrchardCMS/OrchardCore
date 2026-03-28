namespace OrchardCore.OpenSearch.ViewModels;

public class OpenSearchIndexResetDeploymentStepViewModel
{
    public bool IncludeAll { get; set; }
    public string[] IndexNames { get; set; }
    public string[] AllIndexNames { get; set; }
}
