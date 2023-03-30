using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.Contents.AuditTrail.Settings;
using OrchardCore.Contents.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Contents.AuditTrail.Drivers
{
    public class ContentAuditTrailSettingsDisplayDriver : SectionDisplayDriver<ISite, ContentAuditTrailSettings>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public ContentAuditTrailSettingsDisplayDriver(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(ContentAuditTrailSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, AuditTrailPermissions.ManageAuditTrailSettings))
            {
                return null;
            }

            return Initialize<ContentAuditTrailSettingsViewModel>("ContentAuditTrailSettings_Edit", model =>
            {
                model.AllowedContentTypes = section.AllowedContentTypes;
            }).Location("Content:10#Content;5").OnGroup(AuditTrailSettingsGroup.Id);
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentAuditTrailSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, AuditTrailPermissions.ManageAuditTrailSettings))
            {
                return null;
            }

            if (context.GroupId == AuditTrailSettingsGroup.Id)
            {
                var model = new ContentAuditTrailSettings();
                await context.Updater.TryUpdateModelAsync(model, Prefix);
                section.AllowedContentTypes = model.AllowedContentTypes;
            }

            return await EditAsync(section, context);
        }
    }
}
