using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a contract for a localization service.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Returns the default culture of the site.
        /// </summary>
        Task<string> GetDefaultCultureAsync();

        /// <summary>
        /// Returns all the supported cultures of the site. It also contains the default culture.
        /// </summary>
        Task<string[]> GetSupportedCulturesAsync();
    }
}
