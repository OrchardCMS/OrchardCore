using System.Linq;
using System.Threading.Tasks;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.Contents.AuditTrail.Services;
using OrchardCore.Contents.AuditTrail.Services.Models;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.AuditTrail.Services
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class AuditTrailContentTypesEventHandler : IAuditTrailContentEventHandler
    {
        private readonly ISiteService _siteService;

        public AuditTrailContentTypesEventHandler(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task BuildingAuditTrailEventAsync(BuildingAuditTrailEventContext buildingAuditTrailEventContext)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var auditTrailSettings = siteSettings.As<AuditTrailSettings>();
            var contentType = buildingAuditTrailEventContext.ContentItem.ContentType;

            if (auditTrailSettings.IgnoredContentTypeNames.Contains(contentType))
            {
                buildingAuditTrailEventContext.IsCanceled = true;
            }
        }
    }
}
