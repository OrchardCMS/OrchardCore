using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Facebook.Login.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Login.Drivers
{
    public class FacebookLoginSettingsDisplayDriver : SectionDisplayDriver<ISite, FacebookLoginSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FacebookLoginSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<IDisplayResult> EditAsync(FacebookLoginSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
            {
                return null;
            }

            return Initialize<FacebookLoginSettingsViewModel>("FacebookLoginSettings_Edit", model =>
            {
                model.CallbackPath = settings.CallbackPath.Value;
                model.SaveTokens = settings.SaveTokens;
            }).Location("Content:5").OnGroup(FacebookConstants.Features.Login);
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, FacebookLoginSettings settings, IUpdateModel updater, UpdateEditorContext context)
        {
            if (context.GroupId == FacebookConstants.Features.Login)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
                {
                    return null;
                }

                var model = new FacebookLoginSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.CallbackPath = model.CallbackPath;
                settings.SaveTokens = model.SaveTokens;

                site.QueueReleaseShellContext();
            }

            return await EditAsync(settings, context);
        }
    }
}
