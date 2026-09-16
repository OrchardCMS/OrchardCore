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

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var factories = _serviceProvider.GetServices<IDeploymentStepFactory>().ToDictionary(factory => factory.Name);
        DeploymentPlansModel model;
        try
        {
            model = context.Step.ToObject<DeploymentPlansModel>();
        }
        catch (JsonException)
        {
            context.Errors.Add(S["Invalid deployment plan recipe structure. No changes have been made."]);
            return;
        }
        if (model?.Plans is null)
        {
            context.Errors.Add(S["A deployment plan array is required. No changes have been made."]);
            return;
        }
        var plans = new List<DeploymentPlan>();
        foreach (var plan in model.Plans)
        {
            if (plan is null)
            {
                context.Errors.Add(S["Every deployment plan must be an object."]);
                continue;
            }
            var candidate = new DeploymentPlan { Name = plan.Name };
            foreach (var step in plan.Steps ?? [])
            {
                if (step?.Step is null || string.IsNullOrWhiteSpace(step.Type) || !factories.TryGetValue(step.Type, out var factory))
                {
                    context.Errors.Add(S["Every deployment step requires an enabled factory type and a configuration object."]);
                    continue;
                }
                try
                {
                    var template = factory.Create();
                    var value = (DeploymentStep)step.Step.ToObject(template.GetType(), _jsonSerializerOptions);
                    value.Name = template.Name;
                    candidate.DeploymentSteps.Add(value);
                }
                catch (JsonException)
                {
                    context.Errors.Add(S["Invalid deployment step configuration. No changes have been made."]);
                }
            }
            plans.Add(candidate);
        }
        foreach (var messages in _deploymentPlanService.ValidateReplacement(plans).Values)
        {
            foreach (var message in messages)
            {
                context.Errors.Add(S[message]);
            }
        }
        if (context.Errors.Count == 0)
        {
            await _deploymentPlanService.CreateOrUpdateDeploymentPlansAsync(plans);
        }
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
