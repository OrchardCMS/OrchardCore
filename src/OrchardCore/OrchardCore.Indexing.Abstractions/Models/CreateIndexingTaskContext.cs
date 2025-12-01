namespace OrchardCore.Indexing.Models;

public sealed class CreateIndexingTaskContext
{
    /// <summary>
    /// The id of the record that is represented by the <see cref="RecordIndexingTask"/> instance.
    /// </summary>
    public string RecordId { get; set; }

    /// <summary>
    /// The category to which this record belongs.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// The type of task.
    /// </summary>
    public RecordIndexingTaskTypes Type { get; set; }

    public CreateIndexingTaskContext(string recordId, string category, RecordIndexingTaskTypes type)
    {
        ArgumentException.ThrowIfNullOrEmpty(recordId);
        ArgumentException.ThrowIfNullOrEmpty(category);

        RecordId = recordId;
        Category = category;
        Type = type;
    }
}
