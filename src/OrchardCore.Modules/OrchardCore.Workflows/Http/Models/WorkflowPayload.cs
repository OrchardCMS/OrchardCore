namespace OrchardCore.Workflows.Http.Models
{
    public class WorkflowPayload
    {
        public WorkflowPayload(string workflowId, string activityId)
        {
            WorkflowId = workflowId;
            ActivityId = activityId;
        }

        public string WorkflowId { get; private set; }
        public string ActivityId { get; private set; }
    }
}
