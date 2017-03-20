using System.Threading.Tasks;
using OrchardCore.Extensions;

namespace Orchard.Themes.Services
{
    public interface ISiteThemeService
    {
        Task<IExtensionInfo> GetSiteThemeAsync();
        Task SetSiteThemeAsync(string themeName);
        Task<string> GetCurrentThemeNameAsync();
    }
}
