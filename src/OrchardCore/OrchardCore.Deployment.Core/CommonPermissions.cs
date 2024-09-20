using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

/// <summary>
/// This class contains the source references to the OrchardCore.Deployment module permissions so they can be used in other modules
/// without having to reference the OrchardCore.Deployment by itself.
/// </summary>
[Obsolete("This will be removed in a future release. Instead use 'OrchardCore.Deployment.DeploymentPermissions'.")]
public static class CommonPermissions
{
    public static readonly Permission ManageDeploymentPlan = DeploymentPermissions.ManageDeploymentPlan;

    public static readonly Permission Export = DeploymentPermissions.ExportData;

    public static readonly Permission Import = DeploymentPermissions.ImportData;
}
