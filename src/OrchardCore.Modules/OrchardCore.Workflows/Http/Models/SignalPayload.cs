namespace OrchardCore.Workflows.Http.Models
{
    public class SignalPayload
    {
        public static SignalPayload ForWorkflowInstance(string signalName, string workflowInstanceId)
        {
            return new SignalPayload(signalName, workflowInstanceId, null);
        }

        public static SignalPayload ForCorrelation(string signalName, string correlationId)
        {
            return new SignalPayload(signalName, null, correlationId);
        }

        public SignalPayload(string signalName, string workflowInstanceId, string correlationId)
        {
            WorkflowInstanceId = workflowInstanceId;
            CorrelationId = correlationId;
            SignalName = signalName;
        }

        public string WorkflowInstanceId { get; private set; }
        public string CorrelationId { get; private set; }
        public string SignalName { get; private set; }
    }
}