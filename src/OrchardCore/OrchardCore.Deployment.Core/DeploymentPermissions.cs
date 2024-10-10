using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public static class DeploymentPermissions
{
    public static readonly Permission ManageDeploymentPlan = new("ManageDeploymentPlan", "Manage deployment plans");

    public static readonly Permission ExportData = new("Export", "Export Data");

    public static readonly Permission ImportData = new("Import", "Import Data", isSecurityCritical: true);

    public static readonly Permission ManageRemoteInstances = new("ManageRemoteInstances", "Manage remote instances");

    public static readonly Permission ManageRemoteClients = new("ManageRemoteClients", "Manage remote clients");

    public static readonly Permission ExportRemoteInstances = new("ExportRemoteInstances", "Export to remote instances");
}
