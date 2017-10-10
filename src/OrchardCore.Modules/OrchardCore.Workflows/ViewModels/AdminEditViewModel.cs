using System.Collections.Generic;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.ViewModels
{
    public class AdminEditViewModel
    {
        public string LocalId { get; set; }
        public bool IsLocal { get; set; }
        public IEnumerable<IActivity> AllActivities { get; set; }
        public WorkflowDefinitionViewModel WorkflowDefinition { get; set; }
    }
}