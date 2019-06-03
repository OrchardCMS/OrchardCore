using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace OrchardCore.LetsEncrypt.Services
{
    public interface IAzureWebAppService
    {
        Task<IWebAppBase> GetWebAppAsync();
        Task<IPagedCollection<IAppServiceCertificate>> GetAppServiceCertificatesAsync();
    }
}
