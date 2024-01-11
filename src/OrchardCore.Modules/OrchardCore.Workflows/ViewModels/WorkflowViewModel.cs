using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowViewModel
    {
        public Workflow Workflow { get; set; }
        public WorkflowType WorkflowType { get; set; }
        public IList<dynamic> ActivityDesignShapes { get; set; }
        public string WorkflowTypeJson { get; set; }
        public string WorkflowJson { get; set; }
    }
}
