using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Content management functionality to deal with Orchard content items and their parts
    /// </summary>
    public interface IContentManager
    {
        /// <summary>
        /// Creates a new content item with the specified type
        /// </summary>
        /// <remarks>
        /// The content item is not yet persisted!
        /// </remarks>
        /// <param name="contentType">The name of the content type</param>
        Task<ContentItem> NewAsync(string contentType);

        /// <summary>
        /// Updates a content item without creating a new version.
        /// </summary>
        /// <param name="contentItem">The existing content item with updated data</param>
        Task UpdateAsync(ContentItem contentItem);

        /// <summary>
        /// Creates (persists) a new content item with the specified version
        /// </summary>
        /// <param name="contentItem">The content instance filled with all necessary data</param>
        /// <param name="options">The version to create the item with</param>
        Task CreateAsync(ContentItem contentItem, VersionOptions options);


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
        /// Validates a content item
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns>The validation <see cref="ContentValidateResult"/> result.</returns>
        Task<ContentValidateResult> ValidateAsync(ContentItem contentItem);

        /// <summary>
        /// Restores a content item
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns>The validation <see cref="ContentValidateResult"/> result.</returns>
        Task<ContentValidateResult> RestoreAsync(ContentItem contentItem);

        /// <summary>
        /// Gets the published content item with the specified id
        /// </summary>
        /// <param name="id">The content item id to load</param>
        Task<ContentItem> GetAsync(string id);

        /// <summary>
        /// Gets the content item with the specified id and version
        /// </summary>
        /// <param name="id">The id content item id to load</param>
        /// <param name="options">The version option</param>
        Task<ContentItem> GetAsync(string id, VersionOptions options);

        /// <summary>
        /// Gets the published content items with the specified ids
        /// </summary>
        /// <param name="contentItemIds">The content item ids to load</param>
        /// <param name="latest">Whether a draft should be loaded if available. <c>false</c> by default.</param>
        /// <remarks>
        /// This method will always issue a database query.
        /// This means that it should be used only to get a list of content items that have not been loaded.
        /// </remarks>
        Task<IEnumerable<ContentItem>> GetAsync(IEnumerable<string> contentItemIds, bool latest = false);

        /// <summary>
        /// Gets the published content items with the specified ids
        /// </summary>
        /// <param name="contentItemIds">The content item ids to load</param>
        /// <param name="options">The version option</param>
        /// <remarks>
        /// This method will always issue a database query.
        /// This means that it should be used only to get a list of content items that have not been loaded.
        /// </remarks>
        Task<IEnumerable<ContentItem>> GetAsync(IEnumerable<string> contentItemIds, VersionOptions options);

        /// <summary>
        /// Gets the content item with the specified version id
        /// </summary>
        /// <param name="contentItemVersionId">The content item version id</param>
        Task<ContentItem> GetVersionAsync(string contentItemVersionId);

        /// <summary>
        /// Triggers the Load events for a content item that was queried directly from the database.
        /// </summary>
        /// <param name="contentItem">The content item </param>
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
        /// Makes a clone of the content item
        /// </summary>
        /// <param name="contentItem">The content item to clone</param>
        /// <returns>Clone of the item</returns>
        Task<ContentItem> CloneAsync(ContentItem contentItem);
    }

    public static class ContentManagerExtensions
    {
        /// <summary>
        /// Creates (persists) a new Published content item
        /// </summary>
        /// <param name="contentManager">The <see cref="IContentManager"/> instance.</param>
        /// <param name="contentItem">The content instance filled with all necessary data</param>
        public static Task CreateAsync(this IContentManager contentManager, ContentItem contentItem)
        {
            return contentManager.CreateAsync(contentItem, VersionOptions.Published);
        }

        public static Task<TAspect> PopulateAspectAsync<TAspect>(this IContentManager contentManager, IContent content) where TAspect : new()
        {
            return contentManager.PopulateAspectAsync(content, new TAspect());
        }

        public static async Task<bool> HasPublishedVersionAsync(this IContentManager contentManager, IContent content)
        {
            if (content.ContentItem == null)
            {
                return false;
            }

            return content.ContentItem.IsPublished() || (await contentManager.GetAsync(content.ContentItem.ContentItemId, VersionOptions.Published) != null);
        }

        public static Task<ContentItemMetadata> GetContentItemMetadataAsync(this IContentManager contentManager, IContent content)
        {
            return contentManager.PopulateAspectAsync<ContentItemMetadata>(content);
        }

        public static async Task<IEnumerable<ContentItem>> LoadAsync(this IContentManager contentManager, IEnumerable<ContentItem> contentItems)
        {
            var results = new List<ContentItem>(contentItems.Count());

            foreach (var contentItem in contentItems)
            {
                results.Add(await contentManager.LoadAsync(contentItem));
            }

            return results;
        }

        public static async IAsyncEnumerable<ContentItem> LoadAsync(this IContentManager contentManager, IAsyncEnumerable<ContentItem> contentItems)
        {
            await foreach (var contentItem in contentItems)
            {
                yield return await contentManager.LoadAsync(contentItem);
            }
        }

        public static async Task<ContentValidateResult> UpdateValidateAndCreateAsync(this IContentManager contentManager, ContentItem contentItem, VersionOptions options)
        {
            await contentManager.UpdateAsync(contentItem);
            var result = await contentManager.ValidateAsync(contentItem);

            if (result.Succeeded)
            {
                await contentManager.CreateAsync(contentItem, options);
            }

            return result;
        }

        /// <summary>
        /// Gets either the published container content item with the specified id, or if the json path supplied gets the contained content item. 
        /// </summary>
        /// <param name="contentManager">The <see cref="IContentManager"/> instance.</param>
        /// <param name="id">The content item id to load</param>
        /// <param name="jsonPath">The json path of the contained content item</param>
        public static Task<ContentItem> GetAsync(this IContentManager contentManager, string id, string jsonPath)
        {
            return contentManager.GetAsync(id, jsonPath, VersionOptions.Published);
        }

        /// <summary>
        /// Gets either the container content item with the specified id and version, or if the json path supplied gets the contained content item.
        /// </summary>
        /// <param name="contentManager">The <see cref="IContentManager"/> instance.</param>
        /// <param name="id">The id content item id to load</param>
        /// <param name="options">The version option</param>
        /// <param name="jsonPath">The json path of the contained content item</param>
        public static async Task<ContentItem> GetAsync(this IContentManager contentManager, string id, string jsonPath, VersionOptions options)
        {
            var contentItem = await contentManager.GetAsync(id, options);

            // It represents a contained content item
            if (!String.IsNullOrEmpty(jsonPath))
            {
                var root = contentItem.Content as JObject;
                contentItem = root.SelectToken(jsonPath)?.ToObject<ContentItem>();

                return contentItem;
            }

            return contentItem;
        }
    }

    public class VersionOptions
    {
        /// <summary>
        /// Gets the latest version.
        /// </summary>
        public static VersionOptions Latest { get { return new VersionOptions { IsLatest = true }; } }

        /// <summary>
        /// Gets the latest published version.
        /// </summary>
        public static VersionOptions Published { get { return new VersionOptions { IsPublished = true }; } }

        /// <summary>
        /// Gets the latest draft version.
        /// </summary>
        public static VersionOptions Draft { get { return new VersionOptions { IsDraft = true }; } }

        /// <summary>
        /// Gets the latest version and creates a new version draft based on it.
        /// </summary>
        public static VersionOptions DraftRequired { get { return new VersionOptions { IsDraft = true, IsDraftRequired = true }; } }

        /// <summary>
        /// Gets all versions.
        /// </summary>
        public static VersionOptions AllVersions { get { return new VersionOptions { IsAllVersions = true }; } }

        public bool IsLatest { get; private set; }
        public bool IsPublished { get; private set; }
        public bool IsDraft { get; private set; }
        public bool IsDraftRequired { get; private set; }
        public bool IsAllVersions { get; private set; }
    }
}
