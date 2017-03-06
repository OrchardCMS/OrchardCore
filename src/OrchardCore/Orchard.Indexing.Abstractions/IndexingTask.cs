using System;

namespace Orchard.Indexing
{
    public enum IndexingTaskTypes
    {
        Update = 0,
        Delete = 1
    }

    public class IndexingTask
    {
        public int Id { get; set; }
        public string ContentItemId { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public IndexingTaskTypes Type { get; set; }
    }
}
