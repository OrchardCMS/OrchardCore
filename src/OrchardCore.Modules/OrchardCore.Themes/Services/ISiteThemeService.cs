using System;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Themes.Services
{
    public interface ISiteThemeService
    {
        Task<IExtensionInfo> GetSiteThemeAsync();

        Task SetSiteThemeAsync(string themeName);

        [Obsolete("This method has been deprecated, please use GetSiteThemeNameAsync() instead.", error: false)]
        async Task<string> GetCurrentThemeNameAsync() => await GetSiteThemeNameAsync();

        Task<string> GetSiteThemeNameAsync();
    }
}
