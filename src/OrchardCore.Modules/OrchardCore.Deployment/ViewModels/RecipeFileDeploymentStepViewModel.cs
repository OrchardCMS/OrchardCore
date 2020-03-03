namespace OrchardCore.Deployment.ViewModels
{
    public class RecipeFileDeploymentStepViewModel
    {
        public string RecipeName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string WebSite { get; set; }
        public string Version { get; set; }
        public bool IsSetupRecipe { get; set; }
        public string Categories { get; set; }
        public string Tags { get; set; }
    }
}
