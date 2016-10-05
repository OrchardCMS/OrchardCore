using System.Threading.Tasks;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes.Services
{
    public interface ISiteThemeService
    {
        Task<ExtensionDescriptor> GetSiteThemeAsync();
        Task SetSiteThemeAsync(string themeName);
        Task<string> GetCurrentThemeNameAsync();
    }
}
