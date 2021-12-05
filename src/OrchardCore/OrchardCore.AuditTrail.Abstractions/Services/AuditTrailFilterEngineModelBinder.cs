using OrchardCore.AuditTrail.Models;
using OrchardCore.Filters.Core;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailFilterEngineModelBinder : FilterEngineModelBinder<AuditTrailEvent>
    {
        public AuditTrailFilterEngineModelBinder(IAuditTrailAdminListFilterParser  parser) : base(parser)
        {
        }
    }
}
