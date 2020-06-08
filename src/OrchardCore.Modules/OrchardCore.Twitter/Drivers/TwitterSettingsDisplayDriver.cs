using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.ViewModels;

namespace OrchardCore.Twitter.Drivers
{
    public class TwitterSettingsDisplayDriver : SectionDisplayDriver<ISite, TwitterSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public TwitterSettingsDisplayDriver(
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

        public override async Task<IDisplayResult> EditAsync(TwitterSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterSignin))
            {
                return null;
            }

            return Initialize<TwitterSettingsViewModel>("TwitterSettings_Edit", model =>
            {
                model.APIKey = settings.ConsumerKey;
                if (!string.IsNullOrWhiteSpace(settings.ConsumerSecret))
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);
                    model.APISecretKey = protector.Unprotect(settings.ConsumerSecret);
                }
                else
                {
                    model.APISecretKey = string.Empty;
                }
                model.AccessToken = settings.AccessToken;
                if (!string.IsNullOrWhiteSpace(settings.AccessTokenSecret))
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);
                    model.AccessTokenSecret = protector.Unprotect(settings.AccessTokenSecret);
                }
                else
                {
                    model.AccessTokenSecret = string.Empty;
                }
            }).Location("Content:5").OnGroup(TwitterConstants.Features.Twitter);
        }

        public override async Task<IDisplayResult> UpdateAsync(TwitterSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == TwitterConstants.Features.Twitter)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitter))
                {
                    return null;
                }

                var model = new TwitterSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);

                    settings.ConsumerKey = model.APIKey;
                    settings.ConsumerSecret = protector.Protect(model.APISecretKey);
                    settings.AccessToken = model.AccessToken;
                    settings.AccessTokenSecret = protector.Protect(model.AccessTokenSecret);
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }
            return await EditAsync(settings, context);
        }
    }
}
