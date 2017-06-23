using System.Collections.Generic;
using Orchard.Workflows.Models;

namespace Orchard.Workflows.ViewModels {

    public class AdminIndexViewModel {
        public IList<WorkflowDefinitionEntry> WorkflowDefinitions { get; set; }
        public AdminIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class WorkflowDefinitionEntry {
        public bool IsChecked { get; set; }

        public int WokflowDefinitionId { get; set; }
        public string Name { get; set; }
    }

    public class AdminIndexOptions {
        public string Search { get; set; }
        public WorkflowDefinitionOrder Order { get; set; }
        public WorkflowDefinitionFilter Filter { get; set; }
        public WorkflowDefinitionBulk BulkAction { get; set; }
    }

    public enum WorkflowDefinitionOrder {
        Name,
        Creation
    }

    public enum WorkflowDefinitionFilter {
        All
    }

    public enum WorkflowDefinitionBulk {
        None,
        Delete
    }
}
