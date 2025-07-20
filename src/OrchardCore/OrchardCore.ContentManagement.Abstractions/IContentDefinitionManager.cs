using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Events;

namespace OrchardCore.ContentManagement.Metadata;

/// <summary>
/// This interface provides each client with
/// a different copy of <see cref="ContentTypeDefinition"/> to work with in case
/// multiple clients do modifications.
/// </summary>
public interface IContentDefinitionManager
{
    Task<IEnumerable<ContentTypeDefinition>> LoadTypeDefinitionsAsync();

    Task<IEnumerable<ContentTypeDefinition>> ListTypeDefinitionsAsync();

    Task<IEnumerable<ContentPartDefinition>> LoadPartDefinitionsAsync();

    Task<IEnumerable<ContentPartDefinition>> ListPartDefinitionsAsync();

    Task<ContentTypeDefinition> LoadTypeDefinitionAsync(string name);

    Task<ContentTypeDefinition> GetTypeDefinitionAsync(string name);

    Task<ContentPartDefinition> LoadPartDefinitionAsync(string name);

    Task<ContentPartDefinition> GetPartDefinitionAsync(string name);

    Task DeleteTypeDefinitionAsync(string name);

    Task DeletePartDefinitionAsync(string name);

    Task StoreTypeDefinitionAsync(ContentTypeDefinition contentTypeDefinition);

    Task StorePartDefinitionAsync(ContentPartDefinition contentPartDefinition);

    /// <summary>
    /// Returns an unique identifier that is updated when content definitions have changed.
    /// </summary>
    Task<string> GetIdentifierAsync();

    // Event trigger methods
    void TriggerContentTypeCreated(ContentTypeCreatedContext context);
    void TriggerContentTypeUpdated(ContentTypeUpdatedContext context);
    void TriggerContentTypeRemoved(ContentTypeRemovedContext context);
    void TriggerContentPartCreated(ContentPartCreatedContext context);
    void TriggerContentPartUpdated(ContentPartUpdatedContext context);
    void TriggerContentPartRemoved(ContentPartRemovedContext context);
    void TriggerContentPartAttached(ContentPartAttachedContext context);
    void TriggerContentPartDetached(ContentPartDetachedContext context);
    void TriggerContentTypePartUpdated(ContentTypePartUpdatedContext context);
    void TriggerContentFieldAttached(ContentFieldAttachedContext context);
    void TriggerContentFieldDetached(ContentFieldDetachedContext context);
    void TriggerContentPartFieldUpdated(ContentPartFieldUpdatedContext context);
}
