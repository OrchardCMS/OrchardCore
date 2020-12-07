using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Permissions;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.AuditTrail.Drivers
{
    public class AuditTrailTrimmingSettingsDisplayDriver : SectionDisplayDriver<ISite, AuditTrailTrimmingSettings>
    {
        private readonly IHttpContextAccessor _hca;
        private readonly IAuthorizationService _authorizationService;


        public AuditTrailTrimmingSettingsDisplayDriver(IHttpContextAccessor hca, IAuthorizationService authorizationService)
        {
            _hca = hca;
            _authorizationService = authorizationService;
        }


        public override async Task<IDisplayResult> EditAsync(AuditTrailTrimmingSettings settings, BuildEditorContext context) =>
            !await IsAuthorizedToManageAuditTrailSettingsAsync()
                ? null
                : Initialize<AuditTrailTrimmingSettingsViewModel>("AuditTrailTrimmingSettings_Edit", model =>
                {
                    model.RetentionPeriodDays = settings.RetentionPeriodDays;
                    model.LastRunUtc = settings.LastRunUtc;
                    model.Disabled = settings.Disabled;
                }).Location("Content:10").OnGroup(AuditTrailSettingsDisplayDriver.AuditTrailSettingsGroupId);

        public override async Task<IDisplayResult> UpdateAsync(AuditTrailTrimmingSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == AuditTrailSettingsDisplayDriver.AuditTrailSettingsGroupId)
            {
                if (!await IsAuthorizedToManageAuditTrailSettingsAsync()) return null;

                var model = new AuditTrailTrimmingSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.RetentionPeriodDays = model.RetentionPeriodDays;
                settings.Disabled = model.Disabled;
            }

            return await EditAsync(settings, context);
        }


        private Task<bool> IsAuthorizedToManageAuditTrailSettingsAsync() =>
             _authorizationService.AuthorizeAsync(_hca.HttpContext.User, AuditTrailPermissions.ManageAuditTrailSettings);
    }
}
