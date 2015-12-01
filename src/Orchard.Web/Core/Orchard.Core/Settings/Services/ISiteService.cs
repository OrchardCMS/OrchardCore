using Orchard.DependencyInjection;
using System.Threading.Tasks;

namespace Orchard.Core.Settings.Services
{
    public interface ISiteService : IDependency
    {
        Task<ISite> GetSiteSettingsAsync();
    }
}
