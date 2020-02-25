namespace OrchardCore.Workflows.Http.Models
{
    public class SignalPayload
    {
        public static SignalPayload ForWorkflow(string signalName, string workflowId)
        {
            return new SignalPayload(signalName, workflowId, null);
        }

        public static SignalPayload ForCorrelation(string signalName, string correlationId)
        {
            return new SignalPayload(signalName, null, correlationId);
        }

        public SignalPayload(string signalName, string workflowId, string correlationId)
        {
            WorkflowId = workflowId;
            CorrelationId = correlationId;
            SignalName = signalName;
        }

        public string WorkflowId { get; private set; }
        public string CorrelationId { get; private set; }
        public string SignalName { get; private set; }
    }
}
