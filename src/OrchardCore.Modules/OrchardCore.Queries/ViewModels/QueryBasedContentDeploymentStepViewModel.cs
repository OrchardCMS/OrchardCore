namespace OrchardCore.Queries.ViewModels
{
    public class QueryBasedContentDeploymentStepViewModel
    {
        public string QueryName { get; set; }
        public string QueryParameters { get; set; } = "{}";
        public bool ExportAsSetupRecipe { get; set; }
    }
}
