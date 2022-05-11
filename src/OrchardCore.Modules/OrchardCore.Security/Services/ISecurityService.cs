using System.Threading.Tasks;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Services
{
    public interface ISecurityService
    {
        Task<SecurityHeadersOptions> GetSettingsAsync();
    }
}
