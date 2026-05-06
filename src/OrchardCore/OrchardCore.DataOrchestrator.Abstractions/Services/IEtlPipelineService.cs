using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.Services;

/// <summary>
/// Manages the persistence of ETL pipeline definitions.
/// </summary>
public interface IEtlPipelineService
{
    /// <summary>
    /// Gets a pipeline definition by its pipeline identifier.
    /// </summary>
    Task<EtlPipelineDefinition> GetAsync(string pipelineId);

    /// <summary>
    /// Gets a pipeline definition by its YesSql document identifier.
    /// </summary>
    Task<EtlPipelineDefinition> GetByDocumentIdAsync(long id);

    /// <summary>
    /// Gets all pipeline definitions.
    /// </summary>
    Task<IEnumerable<EtlPipelineDefinition>> ListAsync();

    /// <summary>
    /// Gets all enabled pipeline definitions.
    /// </summary>
    Task<IEnumerable<EtlPipelineDefinition>> ListEnabledAsync();

    /// <summary>
    /// Saves a pipeline definition (creates or updates).
    /// </summary>
    Task SaveAsync(EtlPipelineDefinition pipeline);

    /// <summary>
    /// Deletes a pipeline definition by its pipeline identifier.
    /// </summary>
    Task DeleteAsync(string pipelineId);

    /// <summary>
    /// Saves an execution log entry.
    /// </summary>
    Task SaveLogAsync(EtlExecutionLog log);

    /// <summary>
    /// Gets execution logs for a specific pipeline.
    /// </summary>
    Task<IEnumerable<EtlExecutionLog>> GetLogsAsync(string pipelineId, int count = 20);
}
