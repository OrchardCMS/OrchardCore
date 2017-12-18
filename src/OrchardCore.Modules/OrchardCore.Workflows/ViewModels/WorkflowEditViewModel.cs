using System.Collections.Generic;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowEditViewModel
    {
        public dynamic WorkflowEditor { get; set; }
        public WorkflowDefinitionViewModel WorkflowDefinitionViewModel { get; set; }
        public IList<dynamic> ActivityThumbnails { get; set; }
    }
}