namespace OrchardCore.Workflows.Http.Models
{
    public class WorkflowPathEntry
    {
        public string WorkflowId { get; set; }
        public string ActivityId { get; set; }
        public string HttpMethod { get; set; }
        public string Path { get; set; }
        public string CorrelationId { get; set; }
    }
}
