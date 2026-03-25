using Microsoft.Extensions.Logging;
using OrchardCore.DataOrchestrator.Indexes;
using OrchardCore.DataOrchestrator.Models;
using YesSql;

namespace OrchardCore.DataOrchestrator.Services;

/// <summary>
/// Default implementation of <see cref="IEtlPipelineService"/> using YesSql.
/// </summary>
public sealed class EtlPipelineService : IEtlPipelineService
{
    private readonly ISession _session;
    private readonly ILogger _logger;

    public EtlPipelineService(ISession session, ILogger<EtlPipelineService> logger)
    {
        _session = session;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EtlPipelineDefinition> GetAsync(string pipelineId)
    {
        return await _session.Query<EtlPipelineDefinition, EtlPipelineIndex>(
            x => x.PipelineId == pipelineId).FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<EtlPipelineDefinition> GetByDocumentIdAsync(long id)
    {
        return await _session.GetAsync<EtlPipelineDefinition>(id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EtlPipelineDefinition>> ListAsync()
    {
        return await _session.Query<EtlPipelineDefinition, EtlPipelineIndex>().ListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EtlPipelineDefinition>> ListEnabledAsync()
    {
        return await _session.Query<EtlPipelineDefinition, EtlPipelineIndex>(
            x => x.IsEnabled).ListAsync();
    }

    /// <inheritdoc />
    public async Task SaveAsync(EtlPipelineDefinition pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        await _session.SaveAsync(pipeline);

        _logger.LogDebug("Saved ETL pipeline '{PipelineName}' ({PipelineId}).", pipeline.Name, pipeline.PipelineId);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string pipelineId)
    {
        var pipeline = await GetAsync(pipelineId);

        if (pipeline != null)
        {
            _session.Delete(pipeline);

            _logger.LogInformation("Deleted ETL pipeline '{PipelineName}' ({PipelineId}).", pipeline.Name, pipeline.PipelineId);
        }
    }

    /// <inheritdoc />
    public async Task SaveLogAsync(EtlExecutionLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        await _session.SaveAsync(log);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EtlExecutionLog>> GetLogsAsync(string pipelineId, int count = 20)
    {
        return await _session.Query<EtlExecutionLog, EtlExecutionLogIndex>(
            x => x.PipelineId == pipelineId)
            .OrderByDescending(x => x.StartedUtc)
            .Take(count)
            .ListAsync();
    }
}
