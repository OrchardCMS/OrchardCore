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
        private readonly IShellReleaseManager _shellReleaseManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TwitterSigninSettingsDisplayDriver(
            IShellReleaseManager shellReleaseManager,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _shellReleaseManager = shellReleaseManager;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<IDisplayResult> EditAsync(TwitterSigninSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterSignin))
            {
                return null;
            }

            return Initialize<TwitterSigninSettingsViewModel>("XSigninSettings_Edit", model =>
            {
                if (settings.CallbackPath.HasValue)
                {
                    model.CallbackPath = settings.CallbackPath;
                }
                model.SaveTokens = settings.SaveTokens;
            }).Location("Content:5").OnGroup(TwitterConstants.Features.Signin);
        }
        public override async Task<IDisplayResult> UpdateAsync(TwitterSigninSettings settings, UpdateEditorContext context)
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

                settings.CallbackPath = model.CallbackPath;
                settings.SaveTokens = model.SaveTokens;

                _shellReleaseManager.RequestRelease();
            }

            return await EditAsync(settings, context);
        }
    }
}
