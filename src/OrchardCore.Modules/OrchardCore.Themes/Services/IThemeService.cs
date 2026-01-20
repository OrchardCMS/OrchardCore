using System.Threading.Tasks;

namespace OrchardCore.Themes.Services
{
    public interface IThemeService
    {
        Task DisableThemeFeaturesAsync(string themeName);
        Task EnableThemeFeaturesAsync(string themeName);
    }
}
