namespace OrchardCore.Search.Elasticsearch.ViewModels
{
    public class ElasticIndexDeploymentStepViewModel
    {
        public bool IncludeAll { get; set; }
        public string[] IndexNames { get; set; }
        public string[] AllIndexNames { get; set; }
    }
}
