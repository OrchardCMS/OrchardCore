using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.LetsEncrypt.Settings;

namespace OrchardCore.LetsEncrypt.Services
{
    public class AzureAuthSettingsService : IAzureAuthSettingsService
    {
        private readonly IShellHost _orchardHost;
        private LetsEncryptAzureAuthSettings _azureAuthSettings;

        public AzureAuthSettingsService(IShellHost orchardHost)
        {
            _orchardHost = orchardHost;
        }

        public async Task<LetsEncryptAzureAuthSettings> GetAzureAuthSettingsAsync()
        {
            if (_azureAuthSettings != null)
            {
                return _azureAuthSettings;
            }

            // Get the azure auth settings from the default tenant
            var shellSettings = _orchardHost.GetSettings(ShellHelper.DefaultShellName);

            using (var scope = await _orchardHost.GetScopeAsync(shellSettings))
            {
                _azureAuthSettings = scope.ServiceProvider.GetRequiredService<IOptions<LetsEncryptAzureAuthSettings>>().Value;
                return _azureAuthSettings;
            }
        }
    }
}
