using System.Threading.Tasks;
using OrchardCore.Google.TagManager.Settings;

namespace OrchardCore.Google.TagManager.Services
{
    public interface IGoogleTagManagerService
    {
        Task<GoogleTagManagerSettings> GetSettingsAsync();
    }
}
