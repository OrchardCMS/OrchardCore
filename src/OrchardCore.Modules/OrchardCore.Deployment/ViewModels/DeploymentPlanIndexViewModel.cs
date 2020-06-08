using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Deployment.ViewModels
{
    public class DeploymentPlanIndexViewModel
    {
        public IList<DeploymentPlanEntry> DeploymentPlans { get; set; }
        public DeploymentPlanIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class DeploymentPlanEntry
    {
        public DeploymentPlan DeploymentPlan { get; set; }
        public bool IsChecked { get; set; }
    }

    public class DeploymentPlanIndexOptions
    {
        public string Search { get; set; }
        public DeploymentPlansBulkAction BulkAction { get; set; }

        [BindNever]
        public List<SelectListItem> DeploymentPlansBulkAction { get; set; }
    }

    public enum DeploymentPlansBulkAction
    {
        None,
        Delete
    }
}
