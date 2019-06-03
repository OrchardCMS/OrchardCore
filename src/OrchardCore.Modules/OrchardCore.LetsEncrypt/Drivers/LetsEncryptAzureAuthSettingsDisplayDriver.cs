using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.LetsEncrypt.Configuration;
using OrchardCore.LetsEncrypt.Settings;
using OrchardCore.Settings;

namespace OrchardCore.LetsEncrypt.Drivers
{
    public class LetsEncryptAzureAuthSettingsDisplayDriver : SectionDisplayDriver<ISite, LetsEncryptAzureAuthSettings>
    {
        private const string SettingsGroupId = "OrchardCore.LetsEncrypt.Azure.Auth";

        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public LetsEncryptAzureAuthSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IHttpContextAccessor httpContextAccessor,
            IShellHost shellHost,
            ShellSettings shellSettings
            )
        {
            _authorizationService = authorizationService;
            _dataProtectionProvider = dataProtectionProvider;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override async Task<IDisplayResult> EditAsync(LetsEncryptAzureAuthSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageLetsEncryptAzureAuthSettings) || _shellSettings.Name != ShellHelper.DefaultShellName)
            {
                return null;
            }

            return Initialize<LetsEncryptAzureAuthSettings>("LetsEncryptAzureAuthSettings_Edit", model =>
            {
                model.Tenant = settings.Tenant;
                model.SubscriptionId = settings.SubscriptionId;
                model.ClientId = settings.ClientId;
                model.ClientSecret = settings.ClientSecret;
                model.ResourceGroupName = settings.ResourceGroupName;
                model.ServicePlanResourceGroupName = settings.ServicePlanResourceGroupName;
                model.UseIPBasedSSL = settings.UseIPBasedSSL;
                model.WebAppName = settings.WebAppName;
                model.SiteSlotName = settings.SiteSlotName;
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(LetsEncryptAzureAuthSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageLetsEncryptAzureAuthSettings) || _shellSettings.Name != ShellHelper.DefaultShellName)
            {
                return null;
            }

            if (context.GroupId == SettingsGroupId)
            {
                var previousClientSecret = settings.ClientSecret;
                await context.Updater.TryUpdateModelAsync(settings, Prefix);

                // Restore client secret if the input is empty.
                if (string.IsNullOrWhiteSpace(settings.ClientSecret))
                {
                    settings.ClientSecret = previousClientSecret;
                }
                else
                {
                    // encrypt the client secret
                    var protector = _dataProtectionProvider.CreateProtector(nameof(LetsEncryptAzureAuthSettingsConfiguration));
                    settings.ClientSecret = protector.Protect(settings.ClientSecret);
                }

                // Reload the tenant to apply the settings
                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(settings, context);
        }
    }
}