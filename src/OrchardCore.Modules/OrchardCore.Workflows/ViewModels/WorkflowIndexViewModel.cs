using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowIndexViewModel
    {
        public WorkflowType WorkflowType { get; set; }
        public IList<WorkflowEntry> Workflows { get; set; }
        public dynamic Pager { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class WorkflowEntry
    {
        public Workflow Workflow { get; set; }
        public int Id { get; set; }
        public bool IsChecked { get; set; }
    }

    public enum WorkflowBulkAction
    {
        None,
        Delete
    }
}
