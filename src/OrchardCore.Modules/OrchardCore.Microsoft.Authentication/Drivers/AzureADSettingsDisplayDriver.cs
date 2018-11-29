using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using OrchardCore.Facebook.ViewModels;
using OrchardCore.Settings;
using OrchardCore.Microsoft.Authentication.ViewModels;
using System;

namespace OrchardCore.Microsoft.Authentication.Drivers
{
    public class AzureADSettingsDisplayDriver : SectionDisplayDriver<ISite, AzureADSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
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
            _dataProtectionProvider = dataProtectionProvider;
            _clientService = clientService;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override async Task<IDisplayResult> EditAsync(AzureADSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageAuthentication))
            {
                return null;
            }

            return Initialize<AzureADSettingsViewModel>("AzureADSettings_Edit", model =>
            {
                var protector = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.AAD);

                model.AppId = settings.AppId;
                if (!string.IsNullOrWhiteSpace(settings.AppSecret))
                {
                    model.AppSecret = protector.Unprotect(settings.AppSecret);
                }
                if (settings.CallbackPath.HasValue)
                    model.CallbackPath = settings.CallbackPath;
                model.TenantId = settings.TenantId;
                model.Instance = settings.Instance?.AbsoluteUri;

            }).Location("Content:0").OnGroup(MicrosoftAuthenticationConstants.Features.AAD);
        }

        public override async Task<IDisplayResult> UpdateAsync(AzureADSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == MicrosoftAuthenticationConstants.Features.AAD)
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageAuthentication))
                {
                    return null;
                }

                var model = new AzureADSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    var protector = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.AAD);
                    settings.AppId = model.AppId;

                    settings.Instance = new Uri(model.Instance);
                    settings.TenantId = model.TenantId;
                    settings.AppSecret = string.IsNullOrWhiteSpace(model.AppSecret) ? null : protector.Protect(model.AppSecret);
                    settings.CallbackPath = model.CallbackPath;

                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }
            return await EditAsync(settings, context);
        }
    }
}
