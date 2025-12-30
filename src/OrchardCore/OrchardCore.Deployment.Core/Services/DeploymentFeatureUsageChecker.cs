using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Deployment.Core.Services;

public class DeploymentFeatureUsageChecker(
    IDeploymentPlanService deploymentPlanService,
    ITypeFeatureProvider typeFeatureProvider,
    ILogger<DeploymentFeatureUsageChecker> logger) : IFeatureUsageChecker
{
    public async Task<bool> IsFeatureInUseAsync(IFeatureInfo feature)
    {
        var deploymentStepTypeNames = await GetDeploymentStepNamesAsync();

        var featureDeploymentStepTypeNames = typeFeatureProvider.GetTypesForFeature(feature)
                .Where(type => type.IsSubclassOfRawGeneric(typeof(DeploymentSourceBase<>)) && type.GetGenericArguments().Length != 0)
                .Select(type => type.GetGenericArguments()[0].FullName)
                .ToList();

        if (deploymentStepTypeNames.Intersect(featureDeploymentStepTypeNames).Any())
        {
            logger.LogWarning("The feature '{FeatureName}' cannot be disabled because it is used by a deployment plan. Please remove it from the deployment plans before disabling it.", feature.Id);

            return true;
        }

        return false;
    }

    private async Task<IEnumerable<string>> GetDeploymentStepNamesAsync()
    {
        var deploymentPlans = await deploymentPlanService.GetAllDeploymentPlansAsync();

        var deploymentSteps = deploymentPlans.SelectMany(plan => plan.DeploymentSteps);

        return deploymentSteps.Select(step => step.GetType().GenericTypeArguments[0].FullName);
    }
}
