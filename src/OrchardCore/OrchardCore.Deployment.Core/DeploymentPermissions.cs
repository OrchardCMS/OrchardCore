using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public static class DeploymentPermissions
{
    public static readonly Permission ManageDeploymentPlan = new("ManageDeploymentPlan", LocalizedString.Create("Manage deployment plans", typeof(DeploymentPermissions)));

    public static readonly Permission Export = new("Export", LocalizedString.Create("Export Data", typeof(DeploymentPermissions)));

    public static readonly Permission Import = new("Import", LocalizedString.Create("Import Data", typeof(DeploymentPermissions)), isSecurityCritical: true);

    public static readonly Permission ManageRemoteInstances = new("ManageRemoteInstances", LocalizedString.Create("Manage remote instances", typeof(DeploymentPermissions)));

    public static readonly Permission ManageRemoteClients = new("ManageRemoteClients", LocalizedString.Create("Manage remote clients", typeof(DeploymentPermissions)));

    public static readonly Permission ExportRemoteInstances = new("ExportRemoteInstances", LocalizedString.Create("Export to remote instances", typeof(DeploymentPermissions)));
}
