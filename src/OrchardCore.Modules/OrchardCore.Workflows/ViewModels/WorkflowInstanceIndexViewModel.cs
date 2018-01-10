using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowInstanceIndexViewModel
    {
        public WorkflowDefinitionRecord WorkflowDefinition { get; set; }
        public IList<WorkflowInstanceEntry> WorkflowInstances { get; set; }
        public dynamic Pager { get; set; }
    }

    public class WorkflowInstanceEntry
    {
        public WorkflowInstanceRecord WorkflowInstance { get; set; }
        public bool IsChecked { get; set; }
    }

    public enum WorkflowInstanceBulkAction
    {
        None,
        Delete
    }
}
