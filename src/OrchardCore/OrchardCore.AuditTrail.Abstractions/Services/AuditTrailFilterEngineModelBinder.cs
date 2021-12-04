using OrchardCore.AuditTrail.Models;
using OrchardCore.Filters.Core;
using YesSql.Filters.Query;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailFilterEngineModelBinder : FilterEngineModelBinder<AuditTrailEvent>
    {
        public AuditTrailFilterEngineModelBinder(IQueryParser<AuditTrailEvent> parser) : base(parser)
        {
        }
    }
}
