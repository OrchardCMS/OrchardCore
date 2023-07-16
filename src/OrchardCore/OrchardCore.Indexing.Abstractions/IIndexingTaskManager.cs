using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Provides services to create and retrieve <see cref="IndexingTask"/> instances.
    /// It is used by indexers to track all content items that have to be indexed or re-indexed.
    /// </summary>
    public interface IIndexingTaskManager
    {
        /// <summary>
        /// Returns a page of <see cref="IndexingTask"/>.
        /// </summary>
        Task<IEnumerable<IndexingTask>> GetIndexingTasksAsync(long afterTaskId, int count);

        /// <summary>
        /// Creates a new <see cref="IndexingTask"/>.
        /// </summary>
        Task CreateTaskAsync(ContentItem contentItem, IndexingTaskTypes type);
    }
}
