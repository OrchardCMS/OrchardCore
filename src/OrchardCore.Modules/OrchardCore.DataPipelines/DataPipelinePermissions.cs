using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.DataPipelines;

/// <summary>
/// The permissions of data pipelines.
/// </summary>
public static class DataPipelinePermissions
{
    /// <summary>
    /// Create, design, publish and delete pipelines. A published pipeline runs with the access of the user who
    /// published it, so this permission is security critical.
    /// </summary>
    public static readonly Permission ManageDataPipelines = new("ManageDataPipelines", LocalizationSource.Create("Manage data pipelines", typeof(DataPipelinePermissions)), isSecurityCritical: true);

    /// <summary>
    /// Run published pipelines, and cancel their runs.
    /// </summary>
    public static readonly Permission RunDataPipelines = new("RunDataPipelines", LocalizationSource.Create("Run data pipelines", typeof(DataPipelinePermissions)), [ManageDataPipelines]);

    /// <summary>
    /// View pipelines and their runs.
    /// </summary>
    public static readonly Permission ViewDataPipelines = new("ViewDataPipelines", LocalizationSource.Create("View data pipelines and their runs", typeof(DataPipelinePermissions)), [ManageDataPipelines, RunDataPipelines]);
}
