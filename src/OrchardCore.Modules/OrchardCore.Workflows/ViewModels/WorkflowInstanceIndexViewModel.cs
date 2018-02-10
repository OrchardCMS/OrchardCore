using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowInstanceIndexViewModel
    {
        public WorkflowDefinition WorkflowDefinition { get; set; }
        public IList<WorkflowInstanceEntry> WorkflowInstances { get; set; }
        public dynamic Pager { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class WorkflowInstanceEntry
    {
        public WorkflowInstance WorkflowInstance { get; set; }
        public int Id { get; set; }
        public bool IsChecked { get; set; }
    }

    public enum WorkflowInstanceBulkAction
    {
        None,
        Delete
    }
}
