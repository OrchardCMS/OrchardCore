using System.Threading.Tasks;
using OrchardCore.Security.Settings;

namespace OrchardCore.Security.Services
{
    public interface ISecurityService
    {
        Task<SecuritySettings> GetSettingsAsync();
    }
}
