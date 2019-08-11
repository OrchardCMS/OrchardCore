using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{

    public class WorkflowTypeIndexViewModel
    {
        public IList<WorkflowTypeEntry> WorkflowTypes { get; set; }
        public WorkflowTypeIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class WorkflowTypeEntry
    {
        public WorkflowType WorkflowType { get; set; }
        public bool IsChecked { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int WorkflowCount { get; set; }
    }

    public class WorkflowTypeIndexOptions
    {
        public string Search { get; set; }
        public WorkflowTypeOrder Order { get; set; }
        public WorkflowTypeFilter Filter { get; set; }
    }

    public enum WorkflowTypeOrder
    {
        Name,
        Creation
    }

    public enum WorkflowTypeFilter
    {
        All
    }

    public enum WorkflowTypeBulkAction
    {
        None,
        Delete
    }
}
