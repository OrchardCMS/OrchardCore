using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public static class DeploymentPermissions
{
    public static readonly Permission ManageDeploymentPlan = new("ManageDeploymentPlan", new LocalizationSource("Manage deployment plans", typeof(DeploymentPermissions)));

    public static readonly Permission Export = new("Export", new LocalizationSource("Export Data", typeof(DeploymentPermissions)));

    public static readonly Permission Import = new("Import", new LocalizationSource("Import Data", typeof(DeploymentPermissions)), isSecurityCritical: true);

    public static readonly Permission ManageRemoteInstances = new("ManageRemoteInstances", new LocalizationSource("Manage remote instances", typeof(DeploymentPermissions)));

    public static readonly Permission ManageRemoteClients = new("ManageRemoteClients", new LocalizationSource("Manage remote clients", typeof(DeploymentPermissions)));

    public static readonly Permission ExportRemoteInstances = new("ExportRemoteInstances", new LocalizationSource("Export to remote instances", typeof(DeploymentPermissions)));
}
