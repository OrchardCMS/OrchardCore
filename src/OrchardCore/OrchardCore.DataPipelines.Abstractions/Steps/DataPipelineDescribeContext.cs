using System.Security.Claims;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The context of <see cref="IDataPipelineStepType.DescribeAsync(DataPipelineDescribeContext)"/>.
/// </summary>
public sealed class DataPipelineDescribeContext
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<DataField>>> _inputs;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataPipelineDescribeContext"/> class.
    /// </summary>
    /// <param name="step">The step to describe.</param>
    /// <param name="inputs">The fields of each connection of each record input, by input name.</param>
    /// <param name="user">The user the pipeline is designed or run for.</param>
    /// <param name="services">The services of the current scope.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public DataPipelineDescribeContext(
        DataPipelineStep step,
        IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<DataField>>> inputs,
        ClaimsPrincipal user,
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(step);

        Step = step;
        _inputs = inputs ?? new Dictionary<string, IReadOnlyList<IReadOnlyList<DataField>>>();
        User = user;
        Services = services;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the step to describe.
    /// </summary>
    public DataPipelineStep Step { get; }

    /// <summary>
    /// Gets the user the pipeline is designed or run for. Data source steps read schemas for this user.
    /// </summary>
    public ClaimsPrincipal User { get; }

    /// <summary>
    /// Gets the services of the current scope.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets the token to monitor for cancellation requests.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the fields the step writes to each of its record outputs, by output name. Set them while describing.
    /// </summary>
    public IDictionary<string, IReadOnlyList<DataField>> Outputs { get; } = new Dictionary<string, IReadOnlyList<DataField>>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the issues found while describing the step.
    /// </summary>
    public IList<DataPipelineIssue> Issues { get; } = [];

    /// <summary>
    /// Gets the fields of an input. When the input has several connections, they are the fields of the first one.
    /// </summary>
    /// <param name="port">The input name.</param>
    /// <returns>The fields, or an empty list when the input is not connected.</returns>
    public IReadOnlyList<DataField> GetInputFields(string port = DataPipelinePort.Input)
        => _inputs.TryGetValue(port, out var sets) && sets.Count > 0 ? sets[0] : [];

    /// <summary>
    /// Gets the fields of each connection of an input, in the order of the connections.
    /// </summary>
    /// <param name="port">The input name.</param>
    /// <returns>The fields of each connection.</returns>
    public IReadOnlyList<IReadOnlyList<DataField>> GetInputFieldSets(string port = DataPipelinePort.Input)
        => _inputs.TryGetValue(port, out var sets) ? sets : [];

    /// <summary>
    /// Sets the fields of a record output.
    /// </summary>
    /// <param name="fields">The fields.</param>
    /// <param name="port">The output name.</param>
    public void SetOutputFields(IReadOnlyList<DataField> fields, string port = DataPipelinePort.Output)
        => Outputs[port] = fields ?? [];

    /// <summary>
    /// Reports an error that prevents the pipeline from running.
    /// </summary>
    /// <param name="message">The message shown to the user.</param>
    public void AddError(string message)
        => Issues.Add(new DataPipelineIssue(DataPipelineIssueSeverity.Error, message, Step.StepId));

    /// <summary>
    /// Reports a warning, which doesn't prevent the pipeline from running.
    /// </summary>
    /// <param name="message">The message shown to the user.</param>
    public void AddWarning(string message)
        => Issues.Add(new DataPipelineIssue(DataPipelineIssueSeverity.Warning, message, Step.StepId));
}
