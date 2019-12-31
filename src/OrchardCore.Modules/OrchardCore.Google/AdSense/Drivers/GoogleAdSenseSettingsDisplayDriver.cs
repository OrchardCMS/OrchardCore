using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Google.AdSense.Settings;
using OrchardCore.Google.AdSense.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Google.AdSense.Drivers
{
    public class GoogleAdSenseSettingsDisplayDriver : SectionDisplayDriver<ISite, GoogleAdSenseSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GoogleAdSenseSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<IDisplayResult> EditAsync(GoogleAdSenseSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAdSense))
            {
                return null;
            }

            return Initialize<GoogleAdSenseSettingsViewModel>("GoogleAdSenseSettings_Edit", model =>
            {
                model.PublisherID = settings.PublisherID;
            }).Location("Content:5").OnGroup(GoogleConstants.Features.GoogleAdSense);
        }

        public override async Task<IDisplayResult> UpdateAsync(GoogleAdSenseSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == GoogleConstants.Features.GoogleAdSense)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAdSense))
                {
                    return null;
                }

                var model = new GoogleAdSenseSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    settings.PublisherID = model.PublisherID;
                }
            }
            return await EditAsync(settings, context);
        }
    }
}