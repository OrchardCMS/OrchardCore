using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using OrchardCore.Environment.Shell;

namespace OrchardCore.LetsEncrypt.Services
{
    public class AzureWebAppService : IAzureWebAppService
    {
        private IAppServiceManager _appServiceManager;
        private readonly IShellHost _orchardHost;
        private readonly IAzureAuthSettingsService _azureAuthSettingsService;

        public AzureWebAppService(IShellHost orchardHost, IAzureAuthSettingsService azureAuthSettingsService)
        {
            _orchardHost = orchardHost;
            _azureAuthSettingsService = azureAuthSettingsService;
        }

        public async Task<IWebAppBase> GetWebAppAsync()
        {
            var azureAuthSettings = await _azureAuthSettingsService.GetAzureAuthSettingsAsync();
            var appServiceManager = await GetAppServiceManagerAsync();

            var site = appServiceManager.WebApps.GetByResourceGroup(azureAuthSettings.ResourceGroupName, azureAuthSettings.WebAppName);
            var siteOrSlot = (IWebAppBase)site;

            if (!string.IsNullOrEmpty(azureAuthSettings.SiteSlotName))
            {
                var slot = site.DeploymentSlots.GetByName(azureAuthSettings.SiteSlotName);
                siteOrSlot = slot;
            }

            return siteOrSlot;
        }

        public async Task<IPagedCollection<IAppServiceCertificate>> GetAppServiceCertificatesAsync()
        {
            var azureAuthSettings = await _azureAuthSettingsService.GetAzureAuthSettingsAsync();
            var appServiceManager = await GetAppServiceManagerAsync();

            return await appServiceManager.AppServiceCertificates
                .ListByResourceGroupAsync(azureAuthSettings.ServicePlanResourceGroupName ?? azureAuthSettings.ResourceGroupName);
        }

        private async Task<IAppServiceManager> GetAppServiceManagerAsync()
        {
            if (_appServiceManager != null)
            {
                return _appServiceManager;
            }

            var azureAuthSettings = await _azureAuthSettingsService.GetAzureAuthSettingsAsync();

            _appServiceManager = AppServiceManager.Authenticate(await GetAzureCredentialsAsync(), azureAuthSettings.SubscriptionId);

            return _appServiceManager;
        }

        private async Task<AzureCredentials> GetAzureCredentialsAsync()
        {
            var azureAuthSettings = await _azureAuthSettingsService.GetAzureAuthSettingsAsync();

            var servicePrincipalLoginInformation = new ServicePrincipalLoginInformation
            {
                ClientId = azureAuthSettings.ClientId,
                ClientSecret = azureAuthSettings.ClientSecret
            };

            return new AzureCredentials(servicePrincipalLoginInformation, azureAuthSettings.Tenant, AzureEnvironment.AzureGlobalCloud);
        }
    }
}
