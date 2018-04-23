using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowInstanceViewModel
    {
        public Workflow WorkflowInstance { get; set; }
        public WorkflowType WorkflowDefinition { get; set; }
        public IList<dynamic> ActivityDesignShapes { get; set; }
        public string WorkflowDefinitionJson { get; set; }
        public string WorkflowInstanceJson { get; set; }
    }
}