using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Deployment.Core.Services;

/// <summary>
/// Provides functionality to determine whether a feature is currently in use by any deployment plan, preventing its
/// deactivation if it is referenced.
/// </summary>
/// <param name="deploymentPlanService">The service used to retrieve deployment plans for feature usage analysis.</param>
/// <param name="typeFeatureProvider">The provider that maps features to their associated types for inspection.</param>
/// <param name="logger">The logger used to record warnings or informational messages related to feature usage checks.</param>
public class DeploymentFeatureUsageChecker(
    IDeploymentPlanService deploymentPlanService,
    ITypeFeatureProvider typeFeatureProvider,
    ILogger<DeploymentFeatureUsageChecker> logger) : IFeatureUsageChecker
{
    /// <inheritdoc/>
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
