using System.Text.Json.Nodes;

namespace OrchardCore.Deployment.Recipes;

public sealed class DeploymentPlansRecipeStepModel
{
    public DeploymentPlanModel[] Plans { get; set; }
}

public sealed class DeploymentPlanModel
{
    public string Name { get; set; }

    public DeploymentStepModel[] Steps { get; set; }
}

public sealed class DeploymentStepModel
{
    public string Type { get; set; }

    public JsonObject Step { get; set; }
}
