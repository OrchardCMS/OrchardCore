using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Deployment.Core.Services;

/// <summary>
/// Provides functionality to determine whether a feature is disabled and currently in use by any deployment plan, preventing its
/// deactivation if it is referenced.
/// </summary>
public class DeploymentFeatureUsageChecker(
    IDeploymentPlanService deploymentPlanService,
    ITypeFeatureProvider typeFeatureProvider,
    ILogger<DeploymentFeatureUsageChecker> logger) : IFeatureUsageChecker
{
    /// <inheritdoc/>
    public async Task<bool> IsDisabledFeatureInUseAsync(IFeatureInfo feature)
    {
        var deploymentStepTypeNames = await GetDeploymentStepNamesAsync();

        var featureDeploymentSourceTypeNames = typeFeatureProvider.GetTypesForFeature(feature)
            .Where(type => typeof(IDeploymentSource).IsAssignableFrom(type))
            .Select(type => type.FullName);

        if (deploymentStepTypeNames.Any(deploymentStep => featureDeploymentSourceTypeNames.Any(deploymentSource => deploymentSource.Contains(deploymentStep))))
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
