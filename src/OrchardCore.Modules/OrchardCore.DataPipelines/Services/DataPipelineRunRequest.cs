using System.Security.Claims;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Asks for a run of a pipeline's published version.
/// </summary>
public sealed class DataPipelineRunRequest
{
    /// <summary>
    /// The trigger of a run started by a user.
    /// </summary>
    public const string ManualTrigger = "Manual";

    /// <summary>
    /// The trigger of a run started by a workflow.
    /// </summary>
    public const string WorkflowTrigger = "Workflow";

    /// <summary>
    /// Gets or sets what starts the run, such as <see cref="ManualTrigger"/>.
    /// </summary>
    public string Trigger { get; set; } = ManualTrigger;

    /// <summary>
    /// Gets or sets the user who starts the run, if any.
    /// </summary>
    public ClaimsPrincipal TriggeredBy { get; set; }

    /// <summary>
    /// Gets or sets an identifier that links the run to what starts it, such as a workflow instance.
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the values passed to the run.
    /// </summary>
    public IDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets extension data stored on the run, such as the workflow that waits for it.
    /// </summary>
    public Action<Models.DataPipelineRun> Alter { get; set; }
}
