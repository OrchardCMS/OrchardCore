using System.Threading.Tasks;

namespace Orchard.Settings
{
    /// <summary>
    /// Provides services to manage the sites settings.
    /// </summary>
    public interface ISiteService
    {
        /// <summary>
        /// Return the site settings for the current tenant.
        /// </summary>
        Task<ISite> GetSiteSettingsAsync();

        /// <summary>
        /// Persists the changes to the site settings.
        /// </summary>
        Task UpdateSiteSettingsAsync(ISite site);
    }
}
