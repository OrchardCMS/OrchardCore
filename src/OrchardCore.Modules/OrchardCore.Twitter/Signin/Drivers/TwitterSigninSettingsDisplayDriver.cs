using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Twitter.Signin.Settings;
using OrchardCore.Twitter.Signin.ViewModels;

namespace OrchardCore.Twitter.Signin.Drivers
{
    public class TwitterSigninSettingsDisplayDriver : SectionDisplayDriver<ISite, TwitterSigninSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public TwitterSigninSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IShellHost shellHost,
            ShellSettings shellSettings)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override async Task<IDisplayResult> EditAsync(TwitterSigninSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterSignin))
            {
                return null;
            }

            return Initialize<TwitterSigninSettingsViewModel>("TwitterSigninSettings_Edit", model =>
            {
                if (settings.CallbackPath.HasValue)
                {
                    model.CallbackPath = settings.CallbackPath;
                }
                model.SaveTokens = settings.SaveTokens;
            }).Location("Content:5").OnGroup(TwitterConstants.Features.Signin);
        }

        public override async Task<IDisplayResult> UpdateAsync(TwitterSigninSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == TwitterConstants.Features.Signin)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterSignin))
                {
                    return null;
                }

                var model = new TwitterSigninSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    settings.CallbackPath = model.CallbackPath;
                    settings.SaveTokens = model.SaveTokens;
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }
            return await EditAsync(settings, context);
        }
    }
}
