using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowInstanceViewModel
    {
        public WorkflowInstanceRecord WorkflowInstance { get; set; }
        public WorkflowDefinitionRecord WorkflowDefinition { get; set; }
        public IList<dynamic> ActivityDesignShapes { get; set; }
        public string WorkflowDefinitionJson { get; set; }
    }
}