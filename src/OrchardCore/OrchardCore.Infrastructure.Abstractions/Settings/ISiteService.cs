using System.Threading.Tasks;

namespace OrchardCore.Settings
{
    /// <summary>
    /// Provides services to manage the sites settings.
    /// </summary>
    public interface ISiteService
    {
        /// <summary>
        /// Loads the site settings document from the store (or create a new one) for updating and that should not be cached.
        /// </summary>
        Task<ISite> LoadSiteSettingsAsync();

        /// <summary>
        /// Gets the site settings document from the cache (or create a new one) for sharing and that should not be updated.
        /// </summary>
        Task<ISite> GetSiteSettingsAsync();

        /// <summary>
        /// Updates the store with the provided site settings document and then updates the cache.
        /// </summary>
        Task UpdateSiteSettingsAsync(ISite site);
    }
}
