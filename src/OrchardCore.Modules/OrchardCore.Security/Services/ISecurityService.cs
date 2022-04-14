using System.Threading.Tasks;

namespace OrchardCore.Security.Services
{
    public interface ISecurityService
    {
        Task<SecuritySettings> GetSettingsAsync();
    }
}
