using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment
{
    /// <summary>
    /// This class contains the source references to the OrchardCore.Deployment module permissions so they can be used in other modules
    /// without having to reference the OrchardCore.Deployment by itself.
    /// </summary>
    public class CommonPermissions
    {
        public static readonly Permission ManageDeploymentPlan = new("ManageDeploymentPlan", "Manage deployment plans");
        public static readonly Permission Export = new("Export", "Export Data");
        public static readonly Permission Import = new("Import", "Import Data", isSecurityCritical: true);
    }
}
