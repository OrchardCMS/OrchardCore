using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentLocalization
{
    public interface IContentLocalizationManager
    {
        /// <summary>
        /// Get the list of items for the localizationSet
        /// </summary>
        /// <returns>List of all items matching a localizationSet</returns>
        Task<IEnumerable<ContentItem>> GetItemsForSetAsync(string localizationSet);

        /// <summary>
        /// Get the content item that matches the localizationSet / culture combination
        /// </summary>
        /// <returns>ContentItem or null if not found</returns>
        Task<ContentItem> GetContentItemAsync(string localizationSet, string culture);

        /// <summary>
        /// Localizes the content item to the target culture.
        /// This method will clone the ContentItem of the default locale
        /// and set the culture of the LocalizationPart to the targetCulture.
        /// </summary>
        /// <returns>The localized content item</returns>
        Task<ContentItem> LocalizeAsync(ContentItem content, string targetCulture);

        /// <summary>
        /// Deduplicate the list of contentItems to only keep a single contentItem per LocalizationSet.
        /// Each ContentItem is chosen with the following rules:
        /// - ContentItemId of the current culture for the set
        /// - OR ContentItemId of the default culture for the set
        /// - OR First ContentItemId found in the set
        /// </summary>
        /// <returns>Cleaned list of ContentItem</returns>
        Task<IDictionary<string, ContentItem>> DeduplicateContentItemsAsync(IEnumerable<ContentItem> contentItems);

        /// <summary>
        /// Gets a list of ContentItemId for the LocalizationSet based on some rules
        /// Order of elements is kept.
        /// </summary>
        /// <returns>
        /// List of contentItemId, each chosen with the following rules:
        /// - ContentItemId of the current culture for the set
        /// - OR ContentItemId of the default culture for the set
        /// - OR First ContentItemId found in the set
        /// </returns>
        Task<IDictionary<string, string>> GetFirstItemIdForSetsAsync(IEnumerable<string> localizationSets);
        /// <summary>
        /// Get the ContenItems that match the culture and localizationSet.
        /// A single ContentItem is returned per set if it exists.
        /// </summary>
        Task<IEnumerable<ContentItem>> GetItemsForSetsAsync(IEnumerable<string> localizationSets, string culture);
    }
}
