using Orchard.DisplayManagement.Theming;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
namespace Orchard.Themes.Services
{
    /// <summary>
    /// Provide a fallback theme in case no theme were found or matching the current request.
    /// It uses specifically the SafeMode theme.
    /// </summary>
    public class SafeModeThemeSelector : IThemeSelector
    {
        public SafeModeThemeSelector(IStringLocalizer<SafeModeThemeSelector> stringLocalizer)
        {
            T = stringLocalizer;
        }
        IStringLocalizer T { get; set; }
        public Task<ThemeSelectorResult> GetThemeAsync()
        {
            return Task.FromResult(new ThemeSelectorResult
            {
                Priority = -100,
                ThemeName = "SafeMode"
            });
        }
        public bool CanSet { get { return false; } }
        public void SetTheme(string themeName)
        {}

        public string Tag { get { return string.Empty; } }

       public  LocalizedString DisplayName { get { return T["SafeMode"]; } }
        
      public string Name { get { return "safeMode"; } }
    };
}
