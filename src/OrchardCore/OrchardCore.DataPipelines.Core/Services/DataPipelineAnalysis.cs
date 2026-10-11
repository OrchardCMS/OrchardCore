using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// What <see cref="DataPipelineAnalyzer"/> found in a pipeline: its issues, the order its steps run in, and the fields
/// flowing through each step.
/// </summary>
public sealed class DataPipelineAnalysis
{
    private readonly Dictionary<string, DataPipelineStepAnalysis> _steps;

    internal DataPipelineAnalysis(
        IReadOnlyList<DataPipelineIssue> issues,
        IReadOnlyList<string> order,
        Dictionary<string, DataPipelineStepAnalysis> steps)
    {
        Issues = issues;
        Order = order;
        _steps = steps;
    }

    /// <summary>
    /// Gets the issues found.
    /// </summary>
    public IReadOnlyList<DataPipelineIssue> Issues { get; }

    /// <summary>
    /// Gets a value indicating whether an issue prevents the pipeline from running.
    /// </summary>
    public bool HasErrors => Issues.Any(issue => issue.Severity == DataPipelineIssueSeverity.Error);

    /// <summary>
    /// Gets the identifiers of the steps in an order where each step comes after the steps it reads from. Steps that
    /// are part of a loop are left out.
    /// </summary>
    public IReadOnlyList<string> Order { get; }

    /// <summary>
    /// Gets the analysis of every step.
    /// </summary>
    public IReadOnlyCollection<DataPipelineStepAnalysis> Steps => _steps.Values;

    /// <summary>
    /// Gets the analysis of a step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <returns>The analysis, or <see langword="null"/> when the pipeline has no such step.</returns>
    public DataPipelineStepAnalysis GetStep(string stepId)
        => stepId is not null && _steps.TryGetValue(stepId, out var step) ? step : null;
}

/// <summary>
/// What <see cref="DataPipelineAnalyzer"/> found about one step.
/// </summary>
public sealed class DataPipelineStepAnalysis
{
    internal DataPipelineStepAnalysis(DataPipelineStep step, IDataPipelineStepType stepType)
    {
        Step = step;
        StepType = stepType;
    }

    /// <summary>
    /// Gets the step.
    /// </summary>
    public DataPipelineStep Step { get; }

    /// <summary>
    /// Gets the type of the step, or <see langword="null"/> when it is not available.
    /// </summary>
    public IDataPipelineStepType StepType { get; }

    /// <summary>
    /// Gets the inputs of the step.
    /// </summary>
    public IReadOnlyList<DataPipelinePort> InputPorts { get; internal set; } = [];

    /// <summary>
    /// Gets the outputs of the step.
    /// </summary>
    public IReadOnlyList<DataPipelinePort> OutputPorts { get; internal set; } = [];

    /// <summary>
    /// Gets the connections into each input, by input name, in their order.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<DataPipelineConnection>> IncomingConnections { get; internal set; } = new Dictionary<string, IReadOnlyList<DataPipelineConnection>>();

    /// <summary>
    /// Gets the connections out of each output, by output name.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<DataPipelineConnection>> OutgoingConnections { get; internal set; } = new Dictionary<string, IReadOnlyList<DataPipelineConnection>>();

    /// <summary>
    /// Gets the fields of each connection of each record input, by input name.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<DataField>>> Inputs { get; internal set; } = new Dictionary<string, IReadOnlyList<IReadOnlyList<DataField>>>();

    /// <summary>
    /// Gets the fields of each record output, by output name.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<DataField>> Outputs { get; internal set; } = new Dictionary<string, IReadOnlyList<DataField>>();

    /// <summary>
    /// Gets a value indicating whether the step was described, which requires its type to be available and the steps
    /// it reads from to be described without errors.
    /// </summary>
    public bool IsDescribed { get; internal set; }

    /// <summary>
    /// Gets the fields of an input. When the input has several connections, they are the fields of the first one.
    /// </summary>
    /// <param name="port">The input name.</param>
    /// <returns>The fields, or an empty list.</returns>
    public IReadOnlyList<DataField> GetInputFields(string port)
        => Inputs.TryGetValue(port, out var sets) && sets.Count > 0 ? sets[0] : [];
}
