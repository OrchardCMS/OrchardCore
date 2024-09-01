using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI;

public interface IAzureAISearchIndexEvents
{
    /// <summary>
    /// This event is invoked before removing an existing that already exists. 
    /// </summary>
    Task RemovingAsync(AzureAISearchIndexRemoveContext context);

    /// <summary>
    /// This event is invoked after the index is removed.
    /// </summary>
    Task RemovedAsync(AzureAISearchIndexRemoveContext context);

    /// <summary>
    /// This event is invoked before the index is rebuilt.
    /// If the rebuild deletes the index and create a new one, other events like Removing, Removed, Creating, or Created should not be invoked.
    /// </summary>
    Task RebuildingAsync(AzureAISearchIndexRebuildContext context);

    /// <summary>
    /// This event is invoked after the index is rebuilt.
    /// </summary>
    Task RebuiltAsync(AzureAISearchIndexRebuildContext context);

    /// <summary>
    /// This event is invoked before the index is created.
    /// </summary>
    /// <param name="context"></param>
    Task CreatingAsync(AzureAISearchIndexCreateContext context);

    /// <summary>
    /// This event is invoked after the index is created.
    /// </summary>
    Task CreatedAsync(AzureAISearchIndexCreateContext context);
}
