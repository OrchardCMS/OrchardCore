using Microsoft.Extensions.Options;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using YesSql;
using YesSql.Services;
using ISession = YesSql.ISession;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Stores the journal records in the <see cref="WorkflowExecutionRecord.Collection"/> collection.
/// </summary>
public sealed class WorkflowExecutionJournal : IWorkflowExecutionJournal
{
    private const int DeleteBatchSize = 100;

    private readonly ISession _session;
    private readonly WorkflowJournalOptions _options;

    public WorkflowExecutionJournal(ISession session, IOptions<WorkflowJournalOptions> options)
    {
        _session = session;
        _options = options.Value;
    }

    /// <inheritdoc />
    public bool IsEnabled => _options.Enabled;

    /// <inheritdoc />
    public async Task SaveAsync(string workflowId, IEnumerable<WorkflowExecutionRecord> records)
    {
        ArgumentException.ThrowIfNullOrEmpty(workflowId);
        ArgumentNullException.ThrowIfNull(records);

        var saved = 0;

        foreach (var record in records)
        {
            await _session.SaveAsync(record, collection: WorkflowExecutionRecord.Collection);
            saved++;
        }

        if (saved == 0 || _options.MaxRecordsPerInstance <= 0)
        {
            return;
        }

        var count = await _session.QueryIndex<WorkflowExecutionRecordIndex>(index => index.WorkflowId == workflowId, collection: WorkflowExecutionRecord.Collection)
            .CountAsync();
        var excess = count - _options.MaxRecordsPerInstance;

        if (excess <= 0)
        {
            return;
        }

        var oldest = await _session.Query<WorkflowExecutionRecord, WorkflowExecutionRecordIndex>(index => index.WorkflowId == workflowId, collection: WorkflowExecutionRecord.Collection)
            .OrderBy(index => index.Sequence)
            .Take(excess)
            .ListAsync();

        foreach (var record in oldest)
        {
            _session.Delete(record, collection: WorkflowExecutionRecord.Collection);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WorkflowExecutionRecord>> ListAsync(string workflowId, int count = 500)
    {
        if (string.IsNullOrEmpty(workflowId) || count <= 0)
        {
            return [];
        }

        var records = await _session.Query<WorkflowExecutionRecord, WorkflowExecutionRecordIndex>(index => index.WorkflowId == workflowId, collection: WorkflowExecutionRecord.Collection)
            .OrderByDescending(index => index.Sequence)
            .Take(count)
            .ListAsync();

        return records.Reverse().ToList();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(IEnumerable<string> workflowIds)
    {
        ArgumentNullException.ThrowIfNull(workflowIds);

        foreach (var batch in workflowIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().Chunk(DeleteBatchSize))
        {
            var records = await _session.Query<WorkflowExecutionRecord, WorkflowExecutionRecordIndex>(index => index.WorkflowId.IsIn(batch), collection: WorkflowExecutionRecord.Collection)
                .ListAsync();

            foreach (var record in records)
            {
                _session.Delete(record, collection: WorkflowExecutionRecord.Collection);
            }
        }
    }
}
