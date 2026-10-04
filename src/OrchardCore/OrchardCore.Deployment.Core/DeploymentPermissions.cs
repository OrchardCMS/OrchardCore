using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public static class DeploymentPermissions
{
    public static readonly Permission ManageDeploymentPlan = new("ManageDeploymentPlan", LocalizationSource.Create("Manage deployment plans", typeof(DeploymentPermissions)));

    public static readonly Permission Export = new("Export", LocalizationSource.Create("Export Data", typeof(DeploymentPermissions)));

    public static readonly Permission Import = new("Import", LocalizationSource.Create("Import Data", typeof(DeploymentPermissions)), isSecurityCritical: true);

    public static readonly Permission ManageRemoteInstances = new("ManageRemoteInstances", LocalizationSource.Create("Manage remote instances", typeof(DeploymentPermissions)));

    public static readonly Permission ManageRemoteClients = new("ManageRemoteClients", LocalizationSource.Create("Manage remote clients", typeof(DeploymentPermissions)));

    public static readonly Permission ExportRemoteInstances = new("ExportRemoteInstances", LocalizationSource.Create("Export to remote instances", typeof(DeploymentPermissions)));
}
