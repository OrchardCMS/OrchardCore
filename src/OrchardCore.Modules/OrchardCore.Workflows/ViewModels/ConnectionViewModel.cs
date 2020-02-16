namespace OrchardCore.Workflows.ViewModels
{
    public class ConnectionViewModel
    {
        public int Id { get; set; }
        public string SourceClientId { get; set; }
        public string Outcome { get; set; }

        public string DestinationClientId { get; set; }
    }
}
