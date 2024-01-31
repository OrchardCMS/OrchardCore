namespace OrchardCore.Search.Lucene.ViewModels
{
    public class LuceneIndexResetDeploymentStepViewModel
    {
        public bool IncludeAll { get; set; }
        public string[] IndexNames { get; set; }
        public string[] AllIndexNames { get; set; }
    }
}
