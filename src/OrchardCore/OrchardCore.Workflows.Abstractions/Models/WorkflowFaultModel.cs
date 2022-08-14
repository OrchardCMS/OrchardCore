namespace OrchardCore.Workflows.Models
{
    public class WorkflowFaultModel
    {
        public const string WorkflowFaultInputKey = "ErrorInfo";
        public string WorkflowId { get; set; }
        public string ActivityId { get; set; }
        public string ActivityDisplayName { get; set; }
        public string ActivityTypeName { get; set; }
        public string ExceptionDetails { get; set; }
        public string ErrorMessage { get; set; }
        public string FaultMessage { get; set; }
        public string WorkflowName { get; set; }
        public int ExecutedActivityCount { get; set; }
    }
}
