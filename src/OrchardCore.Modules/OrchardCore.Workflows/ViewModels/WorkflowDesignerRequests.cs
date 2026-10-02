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
    /// The left coordinate, in canvas pixels at 100% zoom.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The top coordinate, in canvas pixels at 100% zoom.
    /// </summary>
    public int Y { get; set; }
}
