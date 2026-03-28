namespace OrchardCore.Search.OpenSearch.ViewModels;

public class OpenSearchIndexDeploymentStepViewModel
{
    public bool IncludeAll { get; set; }
    public string[] IndexNames { get; set; }
    public string[] AllIndexNames { get; set; }
}
