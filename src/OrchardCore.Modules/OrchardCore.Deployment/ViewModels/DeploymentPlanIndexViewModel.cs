using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Deployment.ViewModels;

public class DeploymentPlanIndexViewModel
{
    public IList<DeploymentPlanEntry> DeploymentPlans { get; set; }
    public ContentOptions Options { get; set; } = new ContentOptions();
    public dynamic Pager { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the plans, the toolbar and the pager in the configured layout.
    /// </summary>
    public dynamic List { get; set; }
}

public class DeploymentPlanEntry
{
    public DeploymentPlan DeploymentPlan { get; set; }
    public bool IsChecked { get; set; }
}

public class ContentOptions
{
    public string Search { get; set; }
    public ContentsBulkAction BulkAction { get; set; }

    [BindNever]
    public List<SelectListItem> DeploymentPlansBulkAction { get; set; }
}

public enum ContentsBulkAction
{
    None,
    Delete,
}
