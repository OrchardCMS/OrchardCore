using System.Threading.Tasks;

namespace OrchardCore.Settings
{
    /// <summary>
    /// Provides services to manage the sites settings.
    /// </summary>
    public interface ISiteService
    {
        /// <summary>
        /// Loads the site settings from the store for updating and that should not be cached.
        /// </summary>
        Task<ISite> LoadSiteSettingsAsync();

        /// <summary>
        /// Gets the site settings from the cache for sharing and that should not be updated.
        /// </summary>
        Task<ISite> GetSiteSettingsAsync();

        /// <summary>
        /// Updates the store with the provided site settings and then updates the cache.
        /// </summary>
        Task UpdateSiteSettingsAsync(ISite site);
    }
}
