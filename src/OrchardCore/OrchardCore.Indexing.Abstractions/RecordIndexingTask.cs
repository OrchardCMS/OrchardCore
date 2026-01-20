namespace OrchardCore.Indexing;

public enum RecordIndexingTaskTypes
{
    Update = 0,
    Delete = 1,
}

public sealed class RecordIndexingTask
{
    /// <summary>
    /// The unique identifier of the <see cref="RecordIndexingTask"/>.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The id of the record that is represented by the <see cref="RecordIndexingTask"/> instance.
    /// </summary>
    public string RecordId { get; set; }

    /// <summary>
    /// The category to which this record belongs.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// The <see cref="DateTime"/> value the task was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// The type of task.
    /// </summary>
    public RecordIndexingTaskTypes Type { get; set; }
}
