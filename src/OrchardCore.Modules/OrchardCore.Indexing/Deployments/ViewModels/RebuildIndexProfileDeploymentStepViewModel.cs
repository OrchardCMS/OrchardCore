namespace OrchardCore.Search.AzureAI.ViewModels;

public class RebuildIndexProfileDeploymentStepViewModel
{
    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }

    public string[] AllIndexNames { get; set; }
}
