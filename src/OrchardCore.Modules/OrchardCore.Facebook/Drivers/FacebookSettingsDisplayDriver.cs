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
using OrchardCore.Facebook.Settings;
using OrchardCore.Facebook.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers
{
    public class FacebookSettingsDisplayDriver : SectionDisplayDriver<ISite, FacebookSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public FacebookSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IHttpContextAccessor httpContextAccessor,
            IShellHost shellHost,
            ShellSettings shellSettings,
            ILogger<FacebookSettingsDisplayDriver> logger
            )
        {
            _authorizationService = authorizationService;
            _dataProtectionProvider = dataProtectionProvider;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public override async Task<IDisplayResult> EditAsync(FacebookSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
            {
                return null;
            }

            return Initialize<FacebookSettingsViewModel>("FacebookSettings_Edit", model =>
            {
                var protector = _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core);

                model.AppId = settings.AppId;
                model.FBInit = settings.FBInit;
                model.FBInitParams = settings.FBInitParams;
                model.Version = settings.Version;
                model.SdkJs = settings.SdkJs;
                if (!string.IsNullOrWhiteSpace(settings.AppSecret))
                {
                    try
                    {
                        model.AppSecret = protector.Unprotect(settings.AppSecret);
                    }
                    catch (CryptographicException)
                    {
                        _logger.LogError("The app secret could not be decrypted. It may have been encrypted using a different key.");
                        model.AppSecret = string.Empty;
                        model.HasDecryptionError = true;
                    }
                }
            }).Location("Content:0").OnGroup(FacebookConstants.Features.Core);
        }

        public override async Task<IDisplayResult> UpdateAsync(FacebookSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == FacebookConstants.Features.Core)
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
                {
                    return null;
                }

                var model = new FacebookSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    var protector = _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core);
                    settings.AppId = model.AppId;
                    settings.AppSecret = protector.Protect(model.AppSecret);
                    settings.FBInit = model.FBInit;
                    settings.SdkJs = model.SdkJs;
                    if (!string.IsNullOrWhiteSpace(model.FBInitParams))
                        settings.FBInitParams = model.FBInitParams;
                    settings.Version = model.Version;

                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(settings, context);
        }
    }
}
