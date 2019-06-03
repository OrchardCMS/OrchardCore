using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using OrchardCore.LetsEncrypt.Models;

namespace OrchardCore.LetsEncrypt.Services
{
    public class AzureCertificateManager : ICertificateManager
    {
        private readonly IAzureWebAppService _azureWebAppService;
        private readonly IAzureAuthSettingsService _azureAuthSettingsService;

        public AzureCertificateManager(IAzureWebAppService azureWebAppService, IAzureAuthSettingsService azureAuthSettingsService)
        {
            _azureWebAppService = azureWebAppService;
            _azureAuthSettingsService = azureAuthSettingsService;
        }

        public async Task InstallAsync(CertificateInstallModel certInstallModel)
        {
            var azureAuthSettings = await _azureAuthSettingsService.GetAzureAuthSettingsAsync();
            var webApp = await _azureWebAppService.GetWebAppAsync();

            var cert = new CertificateInner()
            {
                PfxBlob = certInstallModel.PfxCertificate,
                Password = string.Empty,
                Location = webApp.Inner.Location,
                ServerFarmId = webApp.Inner.ServerFarmId
            };

            var resourceGroupName = azureAuthSettings.ServicePlanResourceGroupName ?? azureAuthSettings.ResourceGroupName;

            var certOperationResponse = await webApp.Manager.AppServiceCertificates.Inner.CreateOrUpdateWithHttpMessagesAsync(resourceGroupName, certInstallModel.CertInfo.CommonName, cert);

            // TODO handle error response
            //certOperationResponse.Response.IsSuccessStatusCode

            var thumbprint = certOperationResponse.Body.Thumbprint;

            foreach (var hostname in certInstallModel.Hostnames)
            {
                var hostnameBinding = new HostNameBindingInner(
                    azureResourceType: AzureResourceType.Website,
                      hostNameType: HostNameType.Verified,
                      customHostNameDnsRecordType: CustomHostNameDnsRecordType.CName, // TODO: Handle A? Depends on the type of hostname. Need to research this
                      sslState: azureAuthSettings.UseIPBasedSSL ? SslState.IpBasedEnabled : SslState.SniEnabled,
                      thumbprint: thumbprint
                    );

                var hostnameBindingResponse = await webApp.Manager.WebApps.Inner.CreateOrUpdateHostNameBindingWithHttpMessagesAsync(resourceGroupName, azureAuthSettings.WebAppName, hostname, hostnameBinding);

                // TODO: handle error response
            }


        }

        public Task RenewAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
