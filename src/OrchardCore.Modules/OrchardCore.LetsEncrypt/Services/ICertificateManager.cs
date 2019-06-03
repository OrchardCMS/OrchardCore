using System.Threading.Tasks;
using OrchardCore.LetsEncrypt.Models;

namespace OrchardCore.LetsEncrypt.Services
{
    public interface ICertificateManager
    {
        Task InstallAsync(CertificateInstallModel certInstallModel);
        Task RenewAsync();
    }
}
