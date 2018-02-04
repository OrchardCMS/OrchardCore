namespace OrchardCore.Workflows.Models
{
    public class StartResumeWorkflowPayload
    {
        public StartResumeWorkflowPayload(string workflowId, int? activityId = null)
        {
            WorkflowId = workflowId;
            ActivityId = activityId;
        }

        public string WorkflowId { get; private set; }
        public int? ActivityId { get; private set; }
    }
}