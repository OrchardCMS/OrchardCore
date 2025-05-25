using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Indexing.Deployments.ViewModels;

public class IndexEntityDeploymentStepViewModel
{
    public bool IncludeAll { get; set; }

    public SelectListItem[] Indexes { get; set; }
}
