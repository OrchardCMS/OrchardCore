namespace OrchardCore.Search.Lucene.ViewModels
{
    public class LuceneIndexRebuildDeploymentStepViewModel
    {
        public bool IncludeAll { get; set; }
        public string[] IndexNames { get; set; }
        public string[] AllIndexNames { get; set; }
    }
}
