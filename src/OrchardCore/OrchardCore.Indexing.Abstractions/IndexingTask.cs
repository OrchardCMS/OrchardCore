using System;

namespace OrchardCore.Indexing
{
    public enum IndexingTaskTypes
    {
        Update = 0,
        Delete = 1
    }

    public class IndexingTask
    {
        /// <summary>
        /// The unique identifier of the <see cref="IndexingTask"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The id of the content item that is represented by the <see cref="IndexingTask"/> instance.
        /// </summary>
        public string ContentItemId { get; set; }

        /// <summary>
        /// The <see cref="DateTime"/> value the task was created.
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// The type of task.
        /// </summary>
        public IndexingTaskTypes Type { get; set; }
    }
}
