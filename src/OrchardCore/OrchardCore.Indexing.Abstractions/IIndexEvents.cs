using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

public interface IIndexEvents
{
    /// <summary>
    /// This event is invoked before removing an existing that already exists. 
    /// </summary>
    Task RemovingAsync(IndexRemoveContext context);

    /// <summary>
    /// This event is invoked after the index is removed.
    /// </summary>
    Task RemovedAsync(IndexRemoveContext context);

    /// <summary>
    /// This event is invoked before the index is rebuilt.
    /// If the rebuild deletes the index and create a new one, other events like Removing, Removed, Creating, or Created should not be invoked.
    /// </summary>
    Task RebuildingAsync(IndexRebuildContext context);

    /// <summary>
    /// This event is invoked after the index is rebuilt.
    /// </summary>
    Task RebuiltAsync(IndexRebuildContext context);

    /// <summary>
    /// This event is invoked before the index is created.
    /// </summary>
    /// <param name="context"></param>
    Task CreatingAsync(IndexCreateContext context);

    /// <summary>
    /// This event is invoked after the index is created.
    /// </summary>
    Task CreatedAsync(IndexCreateContext context);
}
