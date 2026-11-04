namespace OrchardCore.Indexing.Core;

/// <summary>
/// Counts how many times the documents of an index failed to build, across indexing runs, so
/// that a document that always fails doesn't hold back the cursor of its index forever.
/// </summary>
public interface IIndexingFailureStore
{
    /// <summary>Records a failed attempt and returns how many times the task has failed for the index, or 0 when it can't be recorded.</summary>
    Task<int> RecordFailureAsync(string indexId, long taskId);

    /// <summary>Forgets the failures of an index, once its cursor moved past them or when the index is reset or deleted.</summary>
    Task ClearAsync(string indexId);
}
