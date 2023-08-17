using System.Threading.Tasks;
using OrchardCore.Google.Analytics.Settings;

namespace OrchardCore.Google.Analytics.Services
{
    public interface IGoogleAnalyticsService
    {
        Task<GoogleAnalyticsSettings> GetSettingsAsync();
    }
}
