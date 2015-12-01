using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Models;
using System.Threading.Tasks;

namespace Orchard.Themes.Services
{
    public interface ISiteThemeService : IDependency
    {
        Task<ExtensionDescriptor> GetSiteThemeAsync();
        Task SetSiteThemeAsync(string themeName);
        Task<string> GetCurrentThemeNameAsync();
    }
}
