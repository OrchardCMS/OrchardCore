using System.Text.Json.Serialization;

namespace OrchardCore.Workflows.Models;

/// <summary>
/// How the engine follows an outcome that has several transitions (<see cref="WorkflowType.BranchingMode"/>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<WorkflowBranchingMode>))]
public enum WorkflowBranchingMode
{
    /// <summary>
    /// Only the first transition of an outcome is followed. It's the default, and how workflows always ran.
    /// </summary>
    FirstOnly,

    /// <summary>
    /// Every transition of an outcome is followed, in the order they were added, which runs several branches
    /// without a Fork activity.
    /// </summary>
    All,
}
