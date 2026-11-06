using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// The model of <c>Views/WorkflowType/Designer.cshtml</c>.
/// </summary>
public sealed class WorkflowDesignerViewModel
{
    /// <summary>
    /// The live workflow type.
    /// </summary>
    public WorkflowType WorkflowType { get; init; }

    /// <summary>
    /// The version shown by the read-only version page, if any.
    /// </summary>
    public WorkflowTypeVersion Version { get; init; }

    /// <summary>
    /// The JSON configuration of the designer app, rendered into its <c>data-config</c> attribute.
    /// </summary>
    public string ConfigJson { get; init; }
}
