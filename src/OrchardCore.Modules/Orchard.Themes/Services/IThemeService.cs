using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Themes.Services
{
    public interface IThemeService
    {
        Task DisableThemeFeaturesAsync(string themeName);
        Task EnableThemeFeaturesAsync(string themeName);
    }
}