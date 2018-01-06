namespace OrchardCore.Workflows.Models
{
    public class WorkflowPathEntry
    {
        public int WorkflowDefinitionId { get; set; }
        public int ActivityId { get; set; }
        public string HttpMethod { get; set; }
        public string Path { get; set; }
    }
}
