using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public AuditTrailTrimmingSettingsDisplayDriver(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(AuditTrailTrimmingSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, AuditTrailPermissions.ManageAuditTrailSettings))
            {
                return null;
            }

            return Initialize<AuditTrailTrimmingSettingsViewModel>("AuditTrailTrimmingSettings_Edit", model =>
            {
                model.RetentionDays = section.RetentionDays;
                model.LastRunUtc = section.LastRunUtc;
                model.Disabled = section.Disabled;
            }).Location("Content:10#Trimming;0").OnGroup(AuditTrailSettingsGroup.Id);
        }

        public override async Task<IDisplayResult> UpdateAsync(AuditTrailTrimmingSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, AuditTrailPermissions.ManageAuditTrailSettings))
            {
                return null;
            }

            if (context.GroupId == AuditTrailSettingsGroup.Id)
            {
                var model = new AuditTrailTrimmingSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);
                section.RetentionDays = model.RetentionDays;
                section.Disabled = model.Disabled;
            }

            return await EditAsync(section, context);
        }
    }
}
