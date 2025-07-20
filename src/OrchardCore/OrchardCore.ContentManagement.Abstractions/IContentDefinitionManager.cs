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

    // Content Definition Event Triggers

    /// <summary>
    /// Triggers the ContentTypeCreated event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content type that was created.</param>
    void TriggerContentTypeCreated(ContentTypeCreatedContext context);

    /// <summary>
    /// Triggers the ContentTypeUpdated event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content type that was updated.</param>
    void TriggerContentTypeUpdated(ContentTypeUpdatedContext context);

    /// <summary>
    /// Triggers the ContentTypeRemoved event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content type that was removed.</param>
    void TriggerContentTypeRemoved(ContentTypeRemovedContext context);

    /// <summary>
    /// Triggers the ContentTypeImporting event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content type being imported.</param>
    void TriggerContentTypeImporting(ContentTypeImportingContext context);

    /// <summary>
    /// Triggers the ContentTypeImported event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content type that was imported.</param>
    void TriggerContentTypeImported(ContentTypeImportedContext context);

    /// <summary>
    /// Triggers the ContentPartCreated event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content part that was created.</param>
    void TriggerContentPartCreated(ContentPartCreatedContext context);

    /// <summary>
    /// Triggers the ContentPartUpdated event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content part that was updated.</param>
    void TriggerContentPartUpdated(ContentPartUpdatedContext context);

    /// <summary>
    /// Triggers the ContentPartRemoved event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content part that was removed.</param>
    void TriggerContentPartRemoved(ContentPartRemovedContext context);

    /// <summary>
    /// Triggers the ContentPartAttached event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content part that was attached.</param>
    void TriggerContentPartAttached(ContentPartAttachedContext context);

    /// <summary>
    /// Triggers the ContentPartDetached event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content part that was detached.</param>
    void TriggerContentPartDetached(ContentPartDetachedContext context);

    /// <summary>
    /// Triggers the ContentPartImporting event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content part being imported.</param>
    void TriggerContentPartImporting(ContentPartImportingContext context);

    /// <summary>
    /// Triggers the ContentPartImported event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content part that was imported.</param>
    void TriggerContentPartImported(ContentPartImportedContext context);

    /// <summary>
    /// Triggers the ContentTypePartUpdated event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content type part that was updated.</param>
    void TriggerContentTypePartUpdated(ContentTypePartUpdatedContext context);

    /// <summary>
    /// Triggers the ContentFieldAttached event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content field that was attached.</param>
    void TriggerContentFieldAttached(ContentFieldAttachedContext context);

    /// <summary>
    /// Triggers the ContentFieldUpdated event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content field that was updated.</param>
    void TriggerContentFieldUpdated(ContentFieldUpdatedContext context);

    /// <summary>
    /// Triggers the ContentFieldDetached event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content field that was detached.</param>
    void TriggerContentFieldDetached(ContentFieldDetachedContext context);

    /// <summary>
    /// Triggers the ContentPartFieldUpdated event through registered handlers.
    /// </summary>
    /// <param name="context">The context for the content part field that was updated.</param>
    void TriggerContentPartFieldUpdated(ContentPartFieldUpdatedContext context);
}
