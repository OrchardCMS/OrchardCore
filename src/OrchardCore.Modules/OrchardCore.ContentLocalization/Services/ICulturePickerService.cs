using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.Services
{
    public interface IContentCulturePickerService
    {
        /// <summary>
        /// Get the ContentItemId that matches the url.
        /// </summary>
        /// <returns>ContentItemId or null if not found</returns>
        Task<string> GetContentItemIdFromRouteAsync(PathString url);

        /// <summary>
        /// Get the Localization of the ContentItem from an url.
        /// </summary>
        /// <returns>Culture or null if not found</returns>
        Task<LocalizationEntry> GetLocalizationFromRouteAsync(PathString url);
    }
}
