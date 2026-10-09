namespace OrchardCore.Workflows.Models;

/// <summary>
/// The canvas changes saved into a <see cref="WorkflowTypeDraft"/>: positions, start flags, transitions and
/// removed activities. Activity properties are never part of it.
/// </summary>
public sealed class WorkflowGraphUpdate
{
    /// <summary>
    /// The position and start flag of each activity. Activities that aren't listed keep their current values.
    /// </summary>
    public IList<WorkflowGraphNode> Nodes { get; set; } = [];

    /// <summary>
    /// The complete list of transitions. It replaces the current one.
    /// </summary>
    public IList<Transition> Transitions { get; set; } = [];

    /// <summary>
    /// The identifiers of the activities to remove. They can be restored later with <see cref="RestoredActivityIds"/>.
    /// </summary>
    public IList<string> RemovedActivityIds { get; set; } = [];

    /// <summary>
    /// The identifiers of previously removed activities to bring back, with their properties.
    /// </summary>
    public IList<string> RestoredActivityIds { get; set; } = [];
}

/// <summary>
/// The canvas state of one activity.
/// </summary>
public sealed class WorkflowGraphNode
{
    /// <summary>
    /// The <see cref="ActivityRecord.ActivityId"/>.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The left coordinate, in canvas pixels at 100% zoom.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The top coordinate, in canvas pixels at 100% zoom.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Whether the activity is a start activity.
    /// </summary>
    public bool IsStart { get; set; }
}
