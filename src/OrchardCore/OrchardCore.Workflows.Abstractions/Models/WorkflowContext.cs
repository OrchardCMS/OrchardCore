using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using ActivityRecord = OrchardCore.Workflows.Models.ActivityRecord;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowContext
    {
        public WorkflowDefinitionRecord WorkflowDefinition { get; set; }
        public WorkflowInstanceRecord WorkflowInstance { get; set; }

        public IEnumerable<TransitionRecord> GetInboundTransitions(ActivityRecord activityRecord)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<TransitionRecord> GetOutboundTransitions(ActivityRecord activityRecord)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<TransitionRecord> GetOutboundTransitions(ActivityRecord activityRecord, LocalizedString outcome)
        {
            throw new System.NotImplementedException();
        }
    }
}