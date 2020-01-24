using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Microsoft.Authentication.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Drivers
{
    public class AzureADSettingsDisplayDriver : SectionDisplayDriver<ISite, AzureADSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IAzureADService _clientService;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public AzureADSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IAzureADService clientService,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IShellHost shellHost,
            ShellSettings shellSettings)
        {
            _authorizationService = authorizationService;
            _clientService = clientService;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override async Task<IDisplayResult> EditAsync(AzureADSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
            {
                return null;
            }
            return Initialize<AzureADSettingsViewModel>("AzureADSettings_Edit", model =>
            {
                model.DisplayName = settings.DisplayName;
                model.AppId = settings.AppId;
                model.TenantId = settings.TenantId;
                if (settings.CallbackPath.HasValue)
                {
                    model.CallbackPath = settings.CallbackPath.Value;
                }
            }).Location("Content:0").OnGroup(MicrosoftAuthenticationConstants.Features.AAD);
        }

        public override async Task<IDisplayResult> UpdateAsync(AzureADSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == MicrosoftAuthenticationConstants.Features.AAD)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
                {
                    return null;
                }
                var model = new AzureADSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);
                if (context.Updater.ModelState.IsValid)
                {
                    settings.DisplayName = model.DisplayName;
                    settings.AppId = model.AppId;
                    settings.TenantId = model.TenantId;
                    settings.CallbackPath = model.CallbackPath;
                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }
            return await EditAsync(settings, context);
        }
    }
}
