using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Reads, saves and clears the values pipelines keep from one run to the next, such as how far an incremental read
/// went. Clearing them makes the next run start over, such as reading every row again.
/// </summary>
public sealed class DataPipelineStateManager
{
    private readonly ISession _session;
    private readonly IClock _clock;

    public DataPipelineStateManager(ISession session, IClock clock)
    {
        _session = session;
        _clock = clock;
    }

    /// <summary>
    /// Reads the values the last successful run of a pipeline saved.
    /// </summary>
    /// <param name="pipelineId">The identifier of the pipeline.</param>
    /// <returns>The values; empty when there are none.</returns>
    public async Task<DataPipelineState> GetAsync(string pipelineId)
    {
        var document = await FindAsync(pipelineId);

        return new DataPipelineState(document?.Values);
    }

    /// <summary>
    /// Tells whether a pipeline keeps values from its runs.
    /// </summary>
    /// <param name="pipelineId">The identifier of the pipeline.</param>
    /// <returns><see langword="true"/> when it does.</returns>
    public async Task<bool> HasStateAsync(string pipelineId)
        => (await FindAsync(pipelineId))?.Values.Count > 0;

    /// <summary>
    /// Saves the values of a run.
    /// </summary>
    /// <param name="pipelineId">The identifier of the pipeline.</param>
    /// <param name="state">The values.</param>
    /// <param name="runId">The run that saves them.</param>
    /// <returns>A task that completes when the values are saved.</returns>
    public async Task SaveAsync(string pipelineId, DataPipelineState state, string runId)
    {
        ArgumentException.ThrowIfNullOrEmpty(pipelineId);
        ArgumentNullException.ThrowIfNull(state);

        var document = await FindAsync(pipelineId) ?? new DataPipelineStateDocument { PipelineId = pipelineId };

        document.Values = state.ToDictionary();
        document.ModifiedUtc = _clock.UtcNow;
        document.ModifiedByRunId = runId;

        await _session.SaveAsync(document);
    }

    /// <summary>
    /// Clears the values of a pipeline, so its next run starts over.
    /// </summary>
    /// <param name="pipelineId">The identifier of the pipeline.</param>
    /// <returns>A task that completes when the values are cleared.</returns>
    public async Task ClearAsync(string pipelineId)
    {
        if (await FindAsync(pipelineId) is { } document)
        {
            _session.Delete(document);
        }
    }

    private Task<DataPipelineStateDocument> FindAsync(string pipelineId)
        => string.IsNullOrEmpty(pipelineId)
            ? Task.FromResult<DataPipelineStateDocument>(null)
            : _session.Query<DataPipelineStateDocument, DataPipelineStateIndex>(index => index.PipelineId == pipelineId).FirstOrDefaultAsync();
}
