using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{

    public class WorkflowDefinitionIndexViewModel
    {
        public IList<WorkflowDefinitionEntry> WorkflowDefinitions { get; set; }
        public WorkflowDefinitionIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class WorkflowDefinitionEntry
    {
        public WorkflowDefinition WorkflowDefinition { get; set; }
        public bool IsChecked { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int WorkflowInstanceCount { get; set; }
    }

    public class WorkflowDefinitionIndexOptions
    {
        public string Search { get; set; }
        public WorkflowDefinitionOrder Order { get; set; }
        public WorkflowDefinitionFilter Filter { get; set; }
    }

    public enum WorkflowDefinitionOrder
    {
        Name,
        Creation
    }

    public enum WorkflowDefinitionFilter
    {
        All
    }

    public enum WorkflowDefinitionBulkAction
    {
        None,
        Delete
    }
}
