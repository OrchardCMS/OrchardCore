using System.Threading.Tasks;

namespace OrchardCore.Search.AzureAI;

public interface IAzureAISearchIndexEvents
{
    /// <summary>
    /// This event is invoked before removing an existing that already exists. 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task RemovingAsync(AzureAISearchIndexRemoveContext context);

    /// <summary>
    /// This event is invoked after the index is removed.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task RemovedAsync(AzureAISearchIndexRemoveContext context);

    /// <summary>
    /// This event is invoked before the index is rebuilt.
    /// If the rebuild deletes the index and create a new one, other events like Removing, Removed, Creating, or Created should not be invoked.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task RebuildingAsync(AzureAISearchIndexRebuildContext context);

    Task RebuiltAsync(AzureAISearchIndexRebuildContext context);

    Task CreatingAsync(AzureAISearchIndexCreateContext context);

    Task CreatedAsync(AzureAISearchIndexCreateContext context);
}
