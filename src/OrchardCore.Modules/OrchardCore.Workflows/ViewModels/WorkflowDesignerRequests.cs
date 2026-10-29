using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// A request that only carries the revision the designer last saw.
/// </summary>
public sealed class WorkflowDesignerRevisionRequest
{
    /// <summary>
    /// The revision the designer last saw.
    /// </summary>
    public int Revision { get; set; }
}

/// <summary>
/// Saves the canvas state into the draft.
/// </summary>
public sealed class WorkflowDesignerSaveRequest
{
    /// <summary>
    /// The revision the designer last saw.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// The position and start flag of each activity.
    /// </summary>
    public IList<WorkflowGraphNode> Nodes { get; set; } = [];

    /// <summary>
    /// The complete list of transitions.
    /// </summary>
    public IList<WorkflowDesignerTransition> Transitions { get; set; } = [];

    /// <summary>
    /// The identifiers of the activities that were removed.
    /// </summary>
    public IList<string> RemovedActivityIds { get; set; } = [];

    /// <summary>
    /// The identifiers of removed activities that were brought back, for example by undo.
    /// </summary>
    public IList<string> RestoredActivityIds { get; set; } = [];
}

/// <summary>
/// Adds an activity to the draft.
/// </summary>
public sealed class WorkflowDesignerAddActivityRequest
{
    /// <summary>
    /// The revision the designer last saw.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// The activity type name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The <see cref="Models.ActivityPreset.Id"/> of the preset to add, instead of <see cref="Name"/>.
    /// </summary>
    public string Preset { get; set; }

    /// <summary>
    /// The left coordinate, in canvas pixels at 100% zoom.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The top coordinate, in canvas pixels at 100% zoom.
    /// </summary>
    public int Y { get; set; }
}

/// <summary>
/// The body of the designer's restore request: the version to copy into the draft.
/// </summary>
public sealed class WorkflowDesignerRestoreRequest
{
    /// <summary>
    /// The draft revision the designer last saw.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// The <see cref="Models.WorkflowTypeVersion.VersionId"/> of the version to restore.
    /// </summary>
    public string VersionId { get; set; }
}

/// <summary>
/// The body of the instance viewer's retry request.
/// </summary>
public sealed class WorkflowDesignerRunRequest
{
    /// <summary>
    /// The values of the input variables, by name.
    /// </summary>
    public Dictionary<string, JsonNode> Inputs { get; set; } = [];
}

public sealed class WorkflowDesignerRetryRequest
{
    /// <summary>
    /// The document id of the faulted instance.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    /// The <see cref="ActivityRecord.ActivityId"/> to run the instance from.
    /// </summary>
    public string ActivityId { get; set; }
}

/// <summary>
/// The body of the designer's variables request: every variable of the draft.
/// </summary>
public sealed class WorkflowDesignerVariablesRequest
{
    /// <summary>
    /// The draft revision the designer last saw.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// The variables, which replace those of the draft.
    /// </summary>
    public IList<WorkflowVariableDefinition> Variables { get; set; } = [];
}

/// <summary>
/// The body of the designer's output bindings request: the bindings of one activity.
/// </summary>
public sealed class WorkflowDesignerOutputBindingsRequest
{
    /// <summary>
    /// The draft revision the designer last saw.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// The <see cref="ActivityRecord.ActivityId"/>.
    /// </summary>
    public string ActivityId { get; set; }

    /// <summary>
    /// The variable each output is bound to, by output name; these replace the activity's bindings.
    /// </summary>
    public IDictionary<string, string> Bindings { get; set; } = new Dictionary<string, string>();
}
