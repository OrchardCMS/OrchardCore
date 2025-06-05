using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class ResetIndexProfileDeploymentStepViewModel
{
    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }

    [BindNever]
    public string[] AllIndexNames { get; set; }
}
