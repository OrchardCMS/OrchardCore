using System.Threading.Tasks;

namespace Orchard.Settings
{
    public interface ISiteService
    {
        Task<ISite> GetSiteSettingsAsync();
        Task UpdateSiteSettingsAsync(ISite site);
    }
}
