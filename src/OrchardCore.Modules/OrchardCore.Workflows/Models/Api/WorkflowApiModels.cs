using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Models;

public sealed class WorkflowTypeDto
{
    public string WorkflowTypeId { get; set; }

    [Required]
    public string Name { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsSingleton { get; set; }

    public int LockTimeout { get; set; }

    public int LockExpiration { get; set; }

    public bool DeleteFinishedWorkflows { get; set; }

    public List<ActivityRecordDto> Activities { get; set; } = [];

    public List<TransitionDto> Transitions { get; set; } = [];
}

public sealed class ActivityRecordDto
{
    public string ActivityId { get; set; }

    [Required]
    public string Name { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public bool IsStart { get; set; }

    public JsonObject Properties { get; set; } = [];
}

public sealed class TransitionDto
{
    [Required]
    public string SourceActivityId { get; set; }

    [Required]
    public string SourceOutcomeName { get; set; }

    [Required]
    public string DestinationActivityId { get; set; }
}

public sealed class WorkflowActivityTypeDescriptor
{
    public string Name { get; set; }

    public string DisplayText { get; set; }

    public string Category { get; set; }

    public bool HasEditor { get; set; }

    public string[] Outcomes { get; set; } = [];

    /// <summary>
    /// Gets or sets the JSON Schema for the activity's persisted properties.
    /// </summary>
    /// <remarks>This compatibility alias has the same value as <see cref="PropertiesSchema"/>.</remarks>
    public JsonObject Schema { get; set; } = [];

    /// <summary>
    /// Gets or sets the JSON Schema for the activity's persisted properties.
    /// </summary>
    public JsonObject PropertiesSchema { get; set; } = [];

    /// <summary>
    /// Gets or sets the complete activity record JSON Schema, constrained to this activity type.
    /// </summary>
    public JsonObject ActivityRecordSchema { get; set; } = [];

    /// <summary>
    /// Gets or sets the activity's materialized default property values.
    /// </summary>
    public JsonObject DefaultProperties { get; set; } = [];
}

public sealed class WorkflowInstancesResponse
{
    public int Skip { get; set; }

    public int Take { get; set; }

    public int TotalCount { get; set; }

    public List<Workflow> Items { get; set; } = [];
}

/// <summary>
/// Represents a paged list of workflow resources.
/// </summary>
public sealed class WorkflowListResponse<T>
{
    /// <summary>
    /// Gets or sets the number of workflow resources skipped.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of workflow resources requested.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Gets or sets the total number of matching workflow resources.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the workflow resources in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = [];
}

public sealed class WorkflowGraphValidationResponse
{
    public bool IsValid { get; set; }

    public string[] Errors { get; set; } = [];
}

public sealed class WorkflowExecutionRequest
{
    public string StartActivityId { get; set; }

    public string CorrelationId { get; set; }

    public JsonObject Input { get; set; } = [];
}

public sealed class WorkflowExecutionResponse
{
    public Workflow Workflow { get; set; }

    public JsonObject Output { get; set; } = [];

    public string[] ExecutedActivityIds { get; set; } = [];
}
