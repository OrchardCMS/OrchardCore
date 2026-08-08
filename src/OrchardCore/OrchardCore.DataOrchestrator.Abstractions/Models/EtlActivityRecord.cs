using System.Text.Json.Nodes;

namespace OrchardCore.DataOrchestrator.Models;

/// <summary>
/// Represents a persisted activity configuration within a pipeline, analogous to ActivityRecord.
/// </summary>
public sealed class EtlActivityRecord
{
    /// <summary>
    /// Gets or sets the unique identifier of this activity instance (UUID).
    /// </summary>
    public string ActivityId { get; set; }

    /// <summary>
    /// Gets or sets the activity class name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the X coordinate on the designer canvas.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate on the designer canvas.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets whether this activity is the starting activity.
    /// </summary>
    public bool IsStart { get; set; }

    /// <summary>
    /// Gets or sets the activity-specific properties.
    /// </summary>
    public JsonObject Properties { get; set; } = [];
}
