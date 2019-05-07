using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Twitter.Drivers
{
    public class TwitterSigninSettingsDisplayDriver : SectionDisplayDriver<ISite, TwitterSigninSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public TwitterSigninSettingsDisplayDriver(
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

        public override async Task<IDisplayResult> EditAsync(TwitterSigninSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterSignin))
            {
                return null;
            }

            return Initialize<TwitterSigninSettingsViewModel>("TwitterSigninSettings_Edit", model =>
            {
                model.APIKey = settings.ConsumerKey;
                if (!string.IsNullOrWhiteSpace(settings.ConsumerSecret))
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.TwitterSignin);
                    model.APISecretKey = protector.Unprotect(settings.ConsumerSecret);
                }
                else
                {
                    model.APISecretKey = string.Empty;
                }
                if (settings.CallbackPath.HasValue)
                {
                    model.CallbackPath = settings.CallbackPath;
                }
            }).Location("Content:5").OnGroup(TwitterConstants.Features.TwitterSignin);
        }

        public override async Task<IDisplayResult> UpdateAsync(TwitterSigninSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == TwitterConstants.Features.TwitterSignin)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterSignin))
                {
                    return null;
                }

                var model = new TwitterSigninSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.TwitterSignin);

                    settings.ConsumerKey = model.APIKey;
                    settings.ConsumerSecret = protector.Protect(model.APISecretKey);
                    settings.CallbackPath = model.CallbackPath;
                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }
            return await EditAsync(settings, context);
        }
    }
}