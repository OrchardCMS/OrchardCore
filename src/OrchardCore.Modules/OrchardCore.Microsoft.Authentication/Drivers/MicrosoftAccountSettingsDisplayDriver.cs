using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Microsoft.Authentication.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Drivers
{
    public class MicrosoftAccountSettingsDisplayDriver : SectionDisplayDriver<ISite, MicrosoftAccountSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public MicrosoftAccountSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IHttpContextAccessor httpContextAccessor,
            IShellHost shellHost,
            ShellSettings shellSettings,
            ILogger<MicrosoftAccountSettingsDisplayDriver> logger)
        {
            _authorizationService = authorizationService;
            _dataProtectionProvider = dataProtectionProvider;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public override async Task<IDisplayResult> EditAsync(MicrosoftAccountSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
            {
                return null;
            }

            return Initialize<MicrosoftAccountSettingsViewModel>("MicrosoftAccountSettings_Edit", model =>
            {
                model.AppId = settings.AppId;
                if (!string.IsNullOrWhiteSpace(settings.AppSecret))
                {
                    try
                    {
                        var protector = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount);
                        model.AppSecret = protector.Unprotect(settings.AppSecret);
                    }
                    catch (CryptographicException)
                    {
                        _logger.LogError("The app secret could not be decrypted. It may have been encrypted using a different key.");
                        model.AppSecret = string.Empty;
                        model.HasDecryptionError = true;
                    }
                }
                else
                {
                    model.AppSecret = string.Empty;
                }
                if (settings.CallbackPath.HasValue)
                {
                    model.CallbackPath = settings.CallbackPath.Value;
                }
                model.SaveTokens = settings.SaveTokens;
            }).Location("Content:5").OnGroup(MicrosoftAuthenticationConstants.Features.MicrosoftAccount);
        }

        public override async Task<IDisplayResult> UpdateAsync(MicrosoftAccountSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == MicrosoftAuthenticationConstants.Features.MicrosoftAccount)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
                {
                    return null;
                }

                var model = new MicrosoftAccountSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    var protector = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount);

                    settings.AppId = model.AppId;
                    settings.AppSecret = protector.Protect(model.AppSecret);
                    settings.CallbackPath = model.CallbackPath;
                    settings.SaveTokens = model.SaveTokens;
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }
            return await EditAsync(settings, context);
        }
    }
}
