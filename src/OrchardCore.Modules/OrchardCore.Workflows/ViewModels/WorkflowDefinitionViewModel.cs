using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowDefinitionViewModel
    {
        public WorkflowDefinitionRecord WorkflowDefinition { get; set; }
        public dynamic WorkflowEditor { get; set; }
        public IList<dynamic> ActivityThumbnails { get; set; }
        public string State { get; set; }
    }
}