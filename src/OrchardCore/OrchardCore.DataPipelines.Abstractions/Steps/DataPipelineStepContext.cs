using Microsoft.Extensions.Logging;
using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The context of <see cref="IDataPipelineStepType.ExecuteAsync(DataPipelineStepContext)"/>.
/// </summary>
public sealed class DataPipelineStepContext
{
    private readonly IReadOnlyDictionary<string, DataPipelineInput> _inputs;
    private readonly IReadOnlyDictionary<string, DataPipelineOutput> _outputs;
    private readonly Action<DataPipelineLogEntry> _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataPipelineStepContext"/> class.
    /// </summary>
    /// <param name="step">The step that runs.</param>
    /// <param name="run">The run the step belongs to.</param>
    /// <param name="inputs">The inputs of the step, by name.</param>
    /// <param name="outputs">The outputs of the step, by name.</param>
    /// <param name="metrics">The counters of the step.</param>
    /// <param name="log">Records a message in the run's history.</param>
    /// <param name="cancellationToken">The token that is cancelled when the run is cancelled or fails.</param>
    /// <param name="services">The services the step runs with, or <see langword="null"/> for the services of the run.</param>
    public DataPipelineStepContext(
        DataPipelineStep step,
        DataPipelineRunContext run,
        IReadOnlyDictionary<string, DataPipelineInput> inputs,
        IReadOnlyDictionary<string, DataPipelineOutput> outputs,
        DataPipelineStepMetrics metrics,
        Action<DataPipelineLogEntry> log,
        CancellationToken cancellationToken,
        IServiceProvider services = null)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(run);

        Step = step;
        Run = run;
        _inputs = inputs ?? new Dictionary<string, DataPipelineInput>();
        _outputs = outputs ?? new Dictionary<string, DataPipelineOutput>();
        Metrics = metrics ?? new DataPipelineStepMetrics();
        _log = log;
        CancellationToken = cancellationToken;
        Services = services ?? run.Services;
    }

    /// <summary>
    /// Gets the step that runs.
    /// </summary>
    public DataPipelineStep Step { get; }

    /// <summary>
    /// Gets the run the step belongs to.
    /// </summary>
    public DataPipelineRunContext Run { get; }

    /// <summary>
    /// Gets the counters of the step.
    /// </summary>
    public DataPipelineStepMetrics Metrics { get; }

    /// <summary>
    /// Gets the token that is cancelled when the run is cancelled or fails.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the services the step runs with: its own scope when the host gives each step one, see
    /// <see cref="DataPipelineRunContext.StepScope"/>.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets an input of the step.
    /// </summary>
    /// <param name="port">The input name.</param>
    /// <returns>The input.</returns>
    /// <exception cref="ArgumentException">The step has no input with that name.</exception>
    public DataPipelineInput GetInput(string port = DataPipelinePort.Input)
    {
        return _inputs.TryGetValue(port, out var input)
            ? input
            : throw new ArgumentException($"The step '{Step.StepId}' has no input named '{port}'.", nameof(port));
    }

    /// <summary>
    /// Gets an output of the step.
    /// </summary>
    /// <param name="port">The output name.</param>
    /// <returns>The output.</returns>
    /// <exception cref="ArgumentException">The step has no output with that name.</exception>
    public DataPipelineOutput GetOutput(string port = DataPipelinePort.Output)
    {
        return _outputs.TryGetValue(port, out var output)
            ? output
            : throw new ArgumentException($"The step '{Step.StepId}' has no output named '{port}'.", nameof(port));
    }

    /// <summary>
    /// Records a message in the run's history.
    /// </summary>
    /// <param name="message">The message. It must not contain secrets.</param>
    public void LogInformation(string message)
    {
        _log?.Invoke(new DataPipelineLogEntry { Level = LogLevel.Information, StepId = Step.StepId, Message = message });
    }

    /// <summary>
    /// Records a warning in the run's history, such as a row that could not be converted.
    /// </summary>
    /// <param name="message">The message. It must not contain secrets.</param>
    public void LogWarning(string message)
    {
        Metrics.AddWarning();
        _log?.Invoke(new DataPipelineLogEntry { Level = LogLevel.Warning, StepId = Step.StepId, Message = message });
    }

    /// <summary>
    /// Records something the step delivered, such as a file it saved or sent.
    /// </summary>
    /// <param name="description">What was delivered, and where. It must not contain secrets.</param>
    /// <param name="url">An optional link to what was delivered.</param>
    public void AddDelivery(string description, string url = null)
    {
        Run.AddDelivery(new DataPipelineDelivery
        {
            StepId = Step.StepId,
            Description = description,
            Url = url,
        });

        LogInformation(description);
    }

    /// <summary>
    /// Gets a value the step kept from the last successful run, such as the last value it read.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value, unique within the step.</param>
    /// <returns>The value, or the default value of <typeparamref name="T"/> when there is none.</returns>
    public T GetState<T>(string key)
    {
        return Run.State.Get<T>(GetStateKey(Step.StepId, key));
    }

    /// <summary>
    /// Sets a value the step keeps for the next runs. It is saved when the run succeeds, and never by a preview.
    /// </summary>
    /// <typeparam name="T">The type of the value, serialized as JSON.</typeparam>
    /// <param name="key">The key of the value, unique within the step.</param>
    /// <param name="value">The value; <see langword="null"/> removes it.</param>
    public void SetState<T>(string key, T value)
    {
        Run.State.Set(GetStateKey(Step.StepId, key), value);
    }

    /// <summary>
    /// Gets the key of a value of a step in <see cref="DataPipelineRunContext.State"/>.
    /// </summary>
    /// <param name="stepId">The identifier of the step.</param>
    /// <param name="key">The key of the value within the step.</param>
    /// <returns>The key.</returns>
    public static string GetStateKey(string stepId, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepId);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return $"{stepId}/{key}";
    }
}
