using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Google.Authentication.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Drivers
{
    public class GoogleAuthenticationSettingsDisplayDriver : SectionDisplayDriver<ISite, GoogleAuthenticationSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public GoogleAuthenticationSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IHttpContextAccessor httpContextAccessor,
            IShellHost shellHost,
            ShellSettings shellSettings)
        {
            _authorizationService = authorizationService;
            _dataProtectionProvider = dataProtectionProvider;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override async Task<IDisplayResult> EditAsync(GoogleAuthenticationSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAuthentication))
            {
                return null;
            }

            return Initialize<GoogleAuthenticationSettingsViewModel>("GoogleAuthenticationSettings_Edit", model =>
            {
                model.ClientID = settings.ClientID;
                if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
                {
                    var protector = _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication);
                    model.ClientSecret = protector.Unprotect(settings.ClientSecret);
                }
                else
                {
                    model.ClientSecret = string.Empty;
                }
                if (settings.CallbackPath.HasValue)
                {
                    model.CallbackPath = settings.CallbackPath.Value;
                }
            }).Location("Content:5").OnGroup(GoogleConstants.Features.GoogleAuthentication);
        }

        public override async Task<IDisplayResult> UpdateAsync(GoogleAuthenticationSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == GoogleConstants.Features.GoogleAuthentication)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAuthentication))
                {
                    return null;
                }

                var model = new GoogleAuthenticationSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    var protector = _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication);

                    settings.ClientID = model.ClientID;
                    settings.ClientSecret = protector.Protect(model.ClientSecret);
                    settings.CallbackPath = model.CallbackPath;
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }
            return await EditAsync(settings, context);
        }
    }
}
