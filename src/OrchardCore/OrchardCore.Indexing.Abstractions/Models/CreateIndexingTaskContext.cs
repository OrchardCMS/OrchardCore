namespace OrchardCore.Indexing.Models;

public sealed class CreateIndexingTaskContext
{
    /// <summary>
    /// The id of the record that is represented by the <see cref="IndexingTask"/> instance.
    /// </summary>
    public string RecordId { get; set; }

    /// <summary>
    /// The category to which this record belongs.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// The type of task.
    /// </summary>
    public IndexingTaskTypes Type { get; set; }

    public CreateIndexingTaskContext(string recordId, string category, IndexingTaskTypes type)
    {
        ArgumentException.ThrowIfNullOrEmpty(recordId);
        ArgumentException.ThrowIfNullOrEmpty(category);

        RecordId = recordId;
        Category = category;
        Type = type;
    }
}
