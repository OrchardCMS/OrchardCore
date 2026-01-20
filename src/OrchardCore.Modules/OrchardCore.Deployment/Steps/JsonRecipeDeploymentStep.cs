namespace OrchardCore.Deployment.Steps
{
    /// <summary>
    /// Adds a JSON recipe to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class JsonRecipeDeploymentStep : DeploymentStep
    {
        public JsonRecipeDeploymentStep()
        {
            Name = "JsonRecipe";
        }

        public string Json { get; set; }
    }
}
