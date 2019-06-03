using System.Threading.Tasks;
using OrchardCore.LetsEncrypt.Settings;

namespace OrchardCore.LetsEncrypt.Services
{
    public interface IAzureAuthSettingsService
    {
        Task<LetsEncryptAzureAuthSettings> GetAzureAuthSettingsAsync();
    }
}
