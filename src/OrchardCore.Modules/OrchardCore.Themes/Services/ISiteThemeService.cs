using OrchardCore.Environment.Extensions;

namespace OrchardCore.Themes.Services;

public interface ISiteThemeService
{
    Task<IExtensionInfo> GetSiteThemeAsync();

    Task SetSiteThemeAsync(string themeName);

    Task<string> GetSiteThemeNameAsync();
}
