using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Json;

namespace OrchardCore.Deployment.Deployment;

public class DeploymentPlanDeploymentSource
    : DeploymentSourceBase<DeploymentPlanDeploymentStep>
{
    private readonly IDeploymentPlanService _deploymentPlanService;
    private readonly IEnumerable<IDeploymentStepFactory> _deploymentStepFactories;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public DeploymentPlanDeploymentSource(
        IDeploymentPlanService deploymentPlanService,
        IEnumerable<IDeploymentStepFactory> deploymentStepFactories,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
    {
        _deploymentPlanService = deploymentPlanService;
        _deploymentStepFactories = deploymentStepFactories;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    protected override async Task ProcessAsync(DeploymentPlanDeploymentStep step, DeploymentPlanResult result)
    {
        if (!await _deploymentPlanService.DoesUserHavePermissionsAsync())
        {
            return;
        }

        var deploymentStepFactories = _deploymentStepFactories.ToDictionary(f => f.Name);

        var deploymentPlans = step.IncludeAll
            ? (await _deploymentPlanService.GetAllDeploymentPlansAsync()).ToArray()
            : (await _deploymentPlanService.GetDeploymentPlansAsync(step.DeploymentPlanNames)).ToArray();

        var plans = (from plan in deploymentPlans
                     select new
                     {
                         plan.Name,
                         Steps = (from step in plan.DeploymentSteps
                                  select new
                                  {
                                      Type = GetStepType(deploymentStepFactories, step),
                                      Step = step
                                  }).ToArray(),
                     }).ToArray();

        // Adding deployment plans.
        result.Steps.Add(new JsonObject
        {
            ["name"] = "deployment",
            ["Plans"] = JArray.FromObject(plans, _jsonSerializerOptions),
        });
    }

    /// <summary>
    /// A Site Settings Step is generic and the name is mapped to the <see cref="IDeploymentStepFactory.Name"/> so its 'Type' should be determined though a lookup.
    /// A normal steps name is not mapped to the <see cref="IDeploymentStepFactory.Name"/> and should use its type.
    /// </summary>
    private static string GetStepType(Dictionary<string, IDeploymentStepFactory> deploymentStepFactories, DeploymentStep step)
    {
        if (deploymentStepFactories.TryGetValue(step.Name, out var deploymentStepFactory))
        {
            return deploymentStepFactory.Name;
        }

        return step.GetType().Name;
    }
}
