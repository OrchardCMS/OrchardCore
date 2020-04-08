using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Contents.Deployment.ClickToDeploy
{
    public class ClickToDeploySettingsDisplayDriver : SectionDisplayDriver<ISite, ClickToDeploySettings>
    {
        private const string SettingsGroupId = "ClickToDeploy";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public ClickToDeploySettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(ClickToDeploySettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, OrchardCore.Deployment.CommonPermissions.ManageDeploymentPlan))
            {
                return null;
            }

            return Initialize<ClickToDeploySettingsViewModel>("ClickToDeploySettings_Edit", model =>
            {
                model.ClickToDeployPlanId = settings.ClickToDeployPlanId;
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ClickToDeploySettings settings, BuildEditorContext context)
        {
            if (context.GroupId == SettingsGroupId)
            {
                var model = new ClickToDeploySettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ClickToDeployPlanId);

                settings.ClickToDeployPlanId = model.ClickToDeployPlanId;
            }

            return await EditAsync(settings, context);
        }
    }
}
