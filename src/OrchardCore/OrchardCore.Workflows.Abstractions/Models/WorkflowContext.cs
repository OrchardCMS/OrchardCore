using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using ActivityRecord = OrchardCore.Workflows.Models.Activity;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowContext
    {
        public WorkflowDefinition WorkflowDefinition { get; set; }
        public WorkflowInstance WorkflowInstance { get; set; }

        public IEnumerable<Transition> GetInboundTransitions(ActivityRecord activityRecord)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Transition> GetOutboundTransitions(ActivityRecord activityRecord)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Transition> GetOutboundTransitions(ActivityRecord activityRecord, LocalizedString outcome)
        {
            throw new System.NotImplementedException();
        }
    }
}