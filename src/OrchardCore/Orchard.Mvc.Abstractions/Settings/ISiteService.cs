using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Settings
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

        /// <summary>
        /// Gets a change token that is set when site settings have changed.
        /// </summary>
        IChangeToken ChangeToken { get; }
    }
}
