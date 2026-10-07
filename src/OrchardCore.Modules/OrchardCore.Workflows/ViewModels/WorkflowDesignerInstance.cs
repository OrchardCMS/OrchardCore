using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// The workflow instance shown by the read-only instance viewer.
/// </summary>
public sealed class WorkflowDesignerInstance
{
    /// <summary>
    /// The document identifier of the instance.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// The <see cref="Workflow.WorkflowId"/>.
    /// </summary>
    public string WorkflowId { get; init; }

    /// <summary>
    /// The name of the instance's <see cref="WorkflowStatus"/>.
    /// </summary>
    public string Status { get; init; }

    /// <summary>
    /// The activities the instance waits on.
    /// </summary>
    public IReadOnlyList<string> BlockingActivityIds { get; init; } = [];

    /// <summary>
    /// The stored values of the declared variables, by name.
    /// </summary>
    public IReadOnlyDictionary<string, JsonNode> VariableValues { get; init; } = new Dictionary<string, JsonNode>();
}
