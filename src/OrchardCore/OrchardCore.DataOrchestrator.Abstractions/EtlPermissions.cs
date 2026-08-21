using OrchardCore.Security.Permissions;

namespace OrchardCore.DataOrchestrator;

public static class EtlPermissions
{
    public static readonly Permission ManageEtlPipelines =
        new("ManageEtlPipelines", "Manage ETL pipelines", isSecurityCritical: true);

    public static readonly Permission ExecuteEtlPipelines =
        new("ExecuteEtlPipelines", "Execute ETL pipelines", [ManageEtlPipelines]);

    public static readonly Permission ViewEtlPipelines =
        new("ViewEtlPipelines", "View ETL pipelines and logs", [ManageEtlPipelines]);
}
