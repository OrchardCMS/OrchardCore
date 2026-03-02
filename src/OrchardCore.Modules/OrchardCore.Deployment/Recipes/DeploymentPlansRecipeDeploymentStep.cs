using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Deployment.Recipes;

public sealed class DeploymentPlansRecipeDeploymentStep : RecipeImportStep<DeploymentPlansRecipeStepModel>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IDeploymentPlanService _deploymentPlanService;

    internal readonly IStringLocalizer S;

    public DeploymentPlansRecipeDeploymentStep(
        IServiceProvider serviceProvider,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
        IDeploymentPlanService deploymentPlanService,
        IStringLocalizer<DeploymentPlansRecipeDeploymentStep> stringLocalizer)
    {
        _serviceProvider = serviceProvider;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        _deploymentPlanService = deploymentPlanService;
        S = stringLocalizer;
    }

    public override string Name => "deployment";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .Title("Deployment")
            .Description("Imports deployment plans.")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Title("Deployment Name")),
                ("Plans", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(
                        new RecipeStepSchemaBuilder()
                            .TypeObject()
                            .Properties(
                                ("Name", new RecipeStepSchemaBuilder()
                                    .TypeString()
                                    .Title("Plan Name")),
                                ("Steps", new RecipeStepSchemaBuilder()
                                    .TypeArray()
                                    .Items(
                                        new RecipeStepSchemaBuilder()
                                            .TypeObject()
                                            .Properties(
                                                ("Type", new RecipeStepSchemaBuilder()
                                                    .TypeString()
                                                    .Title("Step Type")),
                                                ("Step", new RecipeStepSchemaBuilder()
                                                    .TypeObject()
                                                    .Properties(
                                                        ("FileName", new RecipeStepSchemaBuilder()
                                                            .TypeString()),
                                                        ("FileContent", new RecipeStepSchemaBuilder()
                                                            .TypeString()),
                                                        ("Id", new RecipeStepSchemaBuilder()
                                                            .TypeString()
                                                            .Title("Step ID")),
                                                        ("Name", new RecipeStepSchemaBuilder()
                                                            .TypeString()
                                                            .Title("Step Name"))
                                                    )
                                                    .Required("Id", "Name")
                                                )
                                            )
                                            .Required("Type", "Step")
                                    )
                                )
                            )
                            .Required("Name", "Steps")
                    )
                )
            )
            .Required("name", "Plans")
            .Build();
    }

    protected override Task ImportAsync(DeploymentPlansRecipeStepModel model, RecipeExecutionContext context)
    {
        var deploymentStepFactories = _serviceProvider.GetServices<IDeploymentStepFactory>().ToDictionary(f => f.Name);

        var unknownTypes = new List<string>();
        var deploymentPlans = new List<DeploymentPlan>();

        foreach (var plan in model.Plans)
        {
            var deploymentPlan = new DeploymentPlan
            {
                Name = plan.Name,
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
}
