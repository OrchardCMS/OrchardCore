using OrchardCore.DisplayManagement.Theming;
using System.Threading.Tasks;

namespace OrchardCore.Themes.Services
{
    /// <summary>
    /// Provide a fallback theme in case no theme were found or matching the current request.
    /// It uses specifically the SafeMode theme.
    /// </summary>
    public class SafeModeThemeSelector : IThemeSelector
    {
        public Task<ThemeSelectorResult> GetThemeAsync()
        {
            return Task.FromResult(new ThemeSelectorResult
            {
                Priority = -100,
                ThemeName = "SafeMode"
            });
        }
    };
}
