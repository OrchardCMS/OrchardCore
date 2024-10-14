using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Deployment.Recipes;

/// <summary>
/// This recipe step creates a deployment plan.
/// </summary>
public sealed class DeploymentPlansRecipeStep : NamedRecipeStepHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IDeploymentPlanService _deploymentPlanService;

    internal readonly IStringLocalizer S;

    public DeploymentPlansRecipeStep(
        IServiceProvider serviceProvider,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
        IDeploymentPlanService deploymentPlanService,
        IStringLocalizer<DeploymentPlansRecipeStep> stringLocalizer)
        : base("deployment")
    {
        _serviceProvider = serviceProvider;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        _deploymentPlanService = deploymentPlanService;
        S = stringLocalizer;
    }

    protected override Task HandleAsync(RecipeExecutionContext context)
    {
        var deploymentStepFactories = _serviceProvider.GetServices<IDeploymentStepFactory>().ToDictionary(f => f.Name);

        var model = context.Step.ToObject<DeploymentPlansModel>();

        var unknownTypes = new List<string>();
        var deploymentPlans = new List<DeploymentPlan>();

        foreach (var plan in model.Plans)
        {
            var deploymentPlan = new DeploymentPlan
            {
                Name = plan.Name
            };

            foreach (var step in plan.Steps)
            {
                if (deploymentStepFactories.TryGetValue(step.Type, out var deploymentStepFactory))
                {
                    var deploymentStep = (DeploymentStep)step.Step.ToObject(deploymentStepFactory.Create().GetType(), _jsonSerializerOptions);

                    deploymentPlan.DeploymentSteps.Add(deploymentStep);
                }
                else
                {
                    unknownTypes.Add(step.Type);
                }
            }

            deploymentPlans.Add(deploymentPlan);
        }

        if (unknownTypes.Count != 0)
        {
            context.Errors.Add(
                S["No changes have been made. The following types of deployment plans cannot be added: {0}. Please ensure that the related features are enabled to add these types of deployment plans.",
                string.Join(", ", unknownTypes)]);

            return Task.CompletedTask;
        }

        return _deploymentPlanService.CreateOrUpdateDeploymentPlansAsync(deploymentPlans);
    }

    private sealed class DeploymentPlansModel
    {
        public DeploymentPlanModel[] Plans { get; set; }
    }

    private sealed class DeploymentPlanModel
    {
        public string Name { get; set; }

        public DeploymentStepModel[] Steps { get; set; }
    }

    private sealed class DeploymentStepModel
    {
        public string Type { get; set; }

        public JsonObject Step { get; set; }
    }
}
