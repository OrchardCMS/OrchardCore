using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

/// <summary>
/// Provides services to create and retrieve <see cref="RecordIndexingTask"/> instances.
/// It is used by indexers to track all content items that have to be indexed or re-indexed.
/// </summary>
public interface IIndexingTaskManager
{
    /// <summary>
    /// Returns a page of <see cref="RecordIndexingTask"/>.
    /// </summary>
    Task<IEnumerable<RecordIndexingTask>> GetIndexingTasksAsync(long afterTaskId, int count, string category);

    /// <summary>
    /// Creates a new <see cref="RecordIndexingTask"/>.
    /// </summary>
    Task CreateTaskAsync(CreateIndexingTaskContext task);
}
