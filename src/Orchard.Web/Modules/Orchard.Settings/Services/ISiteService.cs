using Orchard.DependencyInjection;
using System.Threading.Tasks;

namespace Orchard.Settings.Services
{
    public interface ISiteService : IDependency
    {
        Task<ISite> GetSiteSettingsAsync();
        Task UpdateSiteSettingsAsync(ISite site);
    }
}
