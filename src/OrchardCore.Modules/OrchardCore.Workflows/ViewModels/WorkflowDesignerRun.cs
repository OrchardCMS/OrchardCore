using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// How the designer's Run dialog runs the published version of a workflow type.
/// </summary>
public sealed class WorkflowDesignerRun
{
    /// <summary>
    /// The run starts the workflow with its input variables, on the server.
    /// </summary>
    public const string InputsMode = "inputs";

    /// <summary>
    /// The run sends a request to the URL of the workflow's HTTP Request event.
    /// </summary>
    public const string HttpMode = "http";

    /// <summary>
    /// <see cref="InputsMode"/>, <see cref="HttpMode"/>, or <see langword="null"/> when the published version
    /// starts with another event, or has no start activity.
    /// </summary>
    public string Mode { get; init; }

    /// <summary>
    /// Whether the workflow type is enabled; a disabled one doesn't run.
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// The start activity the run starts on.
    /// </summary>
    public string ActivityId { get; init; }

    /// <summary>
    /// The HTTP method of the HTTP Request event, in <see cref="HttpMode"/>.
    /// </summary>
    public string HttpMethod { get; init; }

    /// <summary>
    /// The input variables of the published version, in <see cref="InputsMode"/>.
    /// </summary>
    public IReadOnlyList<WorkflowVariableDefinition> Inputs { get; init; } = [];
}

/// <summary>
/// What a run from the designer started.
/// </summary>
public sealed class WorkflowDesignerRunResult
{
    /// <summary>
    /// The document id of the instance, or <see langword="null"/> when it was deleted once finished.
    /// </summary>
    public long? InstanceId { get; init; }

    /// <summary>
    /// The URL of the instance's page, when the instance exists.
    /// </summary>
    public string InstanceUrl { get; init; }

    /// <summary>
    /// The name of the instance's <see cref="WorkflowStatus"/>.
    /// </summary>
    public string Status { get; init; }

    public string FaultMessage { get; init; }

    /// <summary>
    /// The values of the output variables, by name.
    /// </summary>
    public IReadOnlyDictionary<string, JsonNode> Outputs { get; init; } = new Dictionary<string, JsonNode>();
}
