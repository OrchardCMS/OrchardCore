using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Services;

/// <summary>
/// Executes ETL pipelines.
/// </summary>
public interface IEtlPipelineExecutor
{
    /// <summary>
    /// Executes the specified pipeline definition.
    /// </summary>
    /// <param name="pipeline">The pipeline definition to execute.</param>
    /// <param name="parameters">Optional parameter values for the execution.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The execution log for the run.</returns>
    Task<EtlExecutionLog> ExecuteAsync(
        EtlPipelineDefinition pipeline,
        IDictionary<string, object> parameters = null,
        CancellationToken cancellationToken = default);
}
