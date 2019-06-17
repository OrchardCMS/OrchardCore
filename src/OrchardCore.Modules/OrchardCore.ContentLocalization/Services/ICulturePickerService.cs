using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentLocalization
{
    public interface IContentCulturePickerService
    {
        /// <summary>
        /// Get the ContentItemId that matches the Url
        /// </summary>
        /// <returns>ContentItemId or null if not found</returns>
        Task<string> GetContentItemIdFromRouteAsync(PathString url);

        /// <summary>
        /// Get the Culture of the ContentItem from a URL
        /// </summary>
        /// <returns>Culture or null if not found</returns>
        Task<string> GetCultureFromRouteAsync(PathString url);

        /// <summary>
        /// Get the ContentItem that that is related to relatedContentItemId via the LocalizationSet and culture
        /// </summary>
        ///  <returns>ContentItem or null if not found</returns>
        Task<ContentItem> GetRelatedContentItemAsync(string contentItemId, string culture);
    }
}
