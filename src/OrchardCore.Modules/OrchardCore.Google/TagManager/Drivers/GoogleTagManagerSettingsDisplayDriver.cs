using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Google.TagManager.Settings;
using OrchardCore.Google.TagManager.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Google.TagManager.Drivers
{
    public class GoogleTagManagerSettingsDisplayDriver : SectionDisplayDriver<ISite, GoogleTagManagerSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GoogleTagManagerSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<IDisplayResult> EditAsync(GoogleTagManagerSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleTagManager))
            {
                return null;
            }

            return Initialize<GoogleTagManagerSettingsViewModel>("GoogleTagManagerSettings_Edit", model =>
            {
                model.ContainerID = settings.ContainerID;
            }).Location("Content:5").OnGroup(GoogleConstants.Features.GoogleTagManager);
        }

        public override async Task<IDisplayResult> UpdateAsync(GoogleTagManagerSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == GoogleConstants.Features.GoogleTagManager)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleTagManager))
                {
                    return null;
                }

                var model = new GoogleTagManagerSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    settings.ContainerID = model.ContainerID;
                }
            }

            return await EditAsync(settings, context);
        }
    }
}
