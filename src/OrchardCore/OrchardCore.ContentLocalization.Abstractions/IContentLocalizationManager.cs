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
        Task<IEnumerable<ContentItem>> GetItemsForSet(string localizationSet);
        /// <summary>
        /// Get the content item that matches the localizationSet / culture combinaison
        /// </summary>
        /// <returns>ContentItem or null if not found</returns>
        Task<ContentItem> GetContentItem(string localizationSet, string culture);
        /// <summary>
        /// Localizes the content item to the target culture.
        /// This method will clone the ContentItem of the default locale
        /// and set the culture of the LocalizationPart to the targetCulture.
        /// </summary>
        /// <returns>The localized content item</returns>
        Task<ContentItem> LocalizeAsync(ContentItem content, string targetCulture);
    }
}
