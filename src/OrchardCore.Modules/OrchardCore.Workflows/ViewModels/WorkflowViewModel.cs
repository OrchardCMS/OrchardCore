using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// The model of <c>Views/Workflow/Details.cshtml</c>, the page of a workflow instance.
/// </summary>
public class WorkflowViewModel
{
    /// <summary>
    /// The workflow instance.
    /// </summary>
    public Workflow Workflow { get; set; }

    /// <summary>
    /// The workflow type the instance runs on.
    /// </summary>
    public WorkflowType WorkflowType { get; set; }

    /// <summary>
    /// The JSON configuration of the read-only workflow designer that shows the instance, rendered into the
    /// <c>data-config</c> attribute of <c>#workflow-designer</c>.
    /// </summary>
    public string DesignerConfigJson { get; set; }

    /// <summary>
    /// The instance as indented JSON, shown on the State tab.
    /// </summary>
    public string WorkflowJson { get; set; }
}
