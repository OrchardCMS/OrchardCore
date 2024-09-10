using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentManagement;

/// <summary>
/// Content management functionality to deal with Orchard content items and their parts.
/// </summary>
public interface IContentManager
{
    /// <summary>
    /// Creates a new content item with the specified type.
    /// </summary>
    /// <remarks>
    /// The content item is not yet persisted!.
    /// </remarks>
    /// <param name="contentType">The name of the content type.</param>
    Task<ContentItem> NewAsync(string contentType);

    /// <summary>
    /// Updates a content item without creating a new version.
    /// </summary>
    /// <param name="contentItem">The existing content item with updated data.</param>
    Task UpdateAsync(ContentItem contentItem);

    /// <summary>
    /// Creates (persists) a new content item with the specified version.
    /// </summary>
    /// <param name="contentItem">The content instance filled with all necessary data.</param>
    /// <param name="options">The version to create the item with.</param>
    Task CreateAsync(ContentItem contentItem, VersionOptions options = null);

    /// <summary>
    /// Creates (puts) a new content item and manages removing and updating existing published or draft items.
    /// </summary>
    /// <param name="contentItem"></param>
    /// <returns>The validation <see cref="ContentValidateResult"/> result.</returns>
    Task<ContentValidateResult> CreateContentItemVersionAsync(ContentItem contentItem);

    /// <summary>
    /// Updates (patches) a content item version.
    /// </summary>
    /// <param name="updatingVersion"></param>
    /// <param name="updatedVersion"></param>
    /// <returns>The validation <see cref="ContentValidateResult"/> result.</returns>
    Task<ContentValidateResult> UpdateContentItemVersionAsync(ContentItem updatingVersion, ContentItem updatedVersion);

    /// <summary>
    /// Imports content items.
    /// </summary>
    /// <param name="contentItems"></param>
    Task ImportAsync(IEnumerable<ContentItem> contentItems);

    /// <summary>
    /// Validates a content item.
    /// </summary>
    /// <param name="contentItem"></param>
    /// <returns>The validation <see cref="ContentValidateResult"/> result.</returns>
    Task<ContentValidateResult> ValidateAsync(ContentItem contentItem);

    /// <summary>
    /// Restores a content item.
    /// </summary>
    /// <param name="contentItem"></param>
    /// <returns>The validation <see cref="ContentValidateResult"/> result.</returns>
    Task<ContentValidateResult> RestoreAsync(ContentItem contentItem);

    /// <summary>
    /// Gets the content item with the specified id and version.
    /// </summary>
    /// <param name="contentItemId">The id of the content item to load.</param>
    /// <param name="options">The version option.</param>
    Task<ContentItem> GetAsync(string contentItemId, VersionOptions options = null);

    /// <summary>
    /// Gets the published content items with the specified ids.
    /// </summary>
    /// <param name="contentItemIds">The content item ids to load.</param>
    /// <param name="options">The version option.</param>
    /// <remarks>
    /// This method will always issue a database query.
    /// This means that it should be used only to get a list of content items that have not been loaded.
    /// </remarks>
    Task<IEnumerable<ContentItem>> GetAsync(IEnumerable<string> contentItemIds, VersionOptions options = null);

    /// <summary>
    /// Gets the content item with the specified version id.
    /// </summary>
    /// <param name="contentItemVersionId">The content item version id.</param>
    Task<ContentItem> GetVersionAsync(string contentItemVersionId);

    /// <summary>
    /// Gets all versions of the given content item id.
    /// </summary>
    /// <param name="contentItemId">The content item id.</param>
    Task<IEnumerable<ContentItem>> GetAllVersionsAsync(string contentItemId);

    /// <summary>
    /// Triggers the Load events for a content item that was queried directly from the database.
    /// </summary>
    /// <param name="contentItem">The content item. </param>
    Task<ContentItem> LoadAsync(ContentItem contentItem);

    /// <summary>
    /// Removes <see cref="ContentItem.Latest"/> and <see cref="ContentItem.Published"/> flags
    /// from a content item, making it invisible from the system. It doesn't physically delete
    /// the content item.
    /// </summary>
    /// <param name="contentItem"></param>
    Task RemoveAsync(ContentItem contentItem);

    /// <summary>
    /// Deletes the draft version of a content item.
    /// </summary>
    /// <param name="contentItem"></param>
    Task DiscardDraftAsync(ContentItem contentItem);

    /// <summary>
    /// Saves the content item if it is a draft version.
    /// </summary>
    /// <param name="contentItem"></param>
    Task SaveDraftAsync(ContentItem contentItem);

    Task PublishAsync(ContentItem contentItem);

    Task UnpublishAsync(ContentItem contentItem);

    Task<TAspect> PopulateAspectAsync<TAspect>(IContent content, TAspect aspect);

    /// <summary>
    /// Makes a clone of the content item.
    /// </summary>
    /// <param name="contentItem">The content item to clone.</param>
    /// <returns>Clone of the item.</returns>
    Task<ContentItem> CloneAsync(ContentItem contentItem);
}
