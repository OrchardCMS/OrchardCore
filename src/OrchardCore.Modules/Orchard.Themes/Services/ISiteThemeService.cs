using System.Threading.Tasks;
using Orchard.Environment.Extensions;

namespace Orchard.Themes.Services
{
    public interface ISiteThemeService
    {
        Task<IExtensionInfo> GetSiteThemeAsync();
        Task SetSiteThemeAsync(string themeName);
        Task<string> GetCurrentThemeNameAsync();
    }
}
