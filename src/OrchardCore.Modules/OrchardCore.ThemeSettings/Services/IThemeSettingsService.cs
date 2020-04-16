using System.Threading.Tasks;
using OrchardCore.ThemeSettings.Models;

namespace OrchardCore.ThemeSettings.Services
{
    public interface IThemeSettingsService
    {
        Task<CustomThemeSettings> GetThemeSettingsAsync();
    }
}
