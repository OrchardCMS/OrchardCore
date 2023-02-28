namespace OrchardCore.Search.Elasticsearch.ViewModels
{
    public class ElasticIndexResetDeploymentStepViewModel
    {
        public bool IncludeAll { get; set; }
        public string[] IndexNames { get; set; }
        public string[] AllIndexNames { get; set; }
    }
}
