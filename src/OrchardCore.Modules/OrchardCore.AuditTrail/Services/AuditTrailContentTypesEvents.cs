using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailContentTypesEvents : IAuditTrailContentEventHandler
    {
        private readonly ISiteService _siteService;


        public AuditTrailContentTypesEvents(ISiteService siteService)
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
