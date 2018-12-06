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
    public class TwitterLoginSettingsDisplayDriver : SectionDisplayDriver<ISite, TwitterLoginSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public TwitterLoginSettingsDisplayDriver(
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

        public override async Task<IDisplayResult> EditAsync(TwitterLoginSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterLogin))
            {
                return null;
            }

            return Initialize<TwitterLoginSettingsViewModel>("TwitterLoginSettings_Edit", model =>
            {
                model.ConsumerKey = settings.ConsumerKey;
                if (!string.IsNullOrWhiteSpace(settings.ConsumerSecret))
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.TwitterLogin);
                    model.ConsumerSecret = protector.Unprotect(settings.ConsumerSecret);
                }
                else
                {
                    model.ConsumerSecret = string.Empty;
                }
                if (settings.CallbackPath.HasValue)
                {
                    model.CallbackPath = settings.CallbackPath;
                }
            }).Location("Content:5").OnGroup(TwitterConstants.Features.TwitterLogin);
        }

        public override async Task<IDisplayResult> UpdateAsync(TwitterLoginSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == TwitterConstants.Features.TwitterLogin)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterLogin))
                {
                    return null;
                }

                var model = new TwitterLoginSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.TwitterLogin);

                    settings.ConsumerKey = model.ConsumerKey;
                    settings.ConsumerSecret = protector.Protect(model.ConsumerSecret);
                    settings.CallbackPath = model.CallbackPath;
                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }
            return await EditAsync(settings, context);
        }
    }
}