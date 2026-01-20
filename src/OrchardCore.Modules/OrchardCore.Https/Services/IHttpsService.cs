using System.Threading.Tasks;
using OrchardCore.Https.Settings;

namespace OrchardCore.Https.Services
{
    public interface IHttpsService
    {
        Task<HttpsSettings> GetSettingsAsync();
    }
}
