using System.Threading.Tasks;

namespace OrchardCore.Settings
{
    /// <summary>
    /// Provides services to manage the sites settings.
    /// </summary>
    public interface ISiteService
    {
        /// <summary>
        /// Returns the site settings of the current tenant for update.
        /// </summary>
        Task<ISite> LoadSiteSettingsAsync();

        /// <summary>
        /// Returns the site settings of the current tenant in read-only.
        /// </summary>
        Task<ISite> GetSiteSettingsAsync();

        /// <summary>
        /// Persists the changes to the site settings.
        /// </summary>
        Task UpdateSiteSettingsAsync(ISite site);
    }
}
