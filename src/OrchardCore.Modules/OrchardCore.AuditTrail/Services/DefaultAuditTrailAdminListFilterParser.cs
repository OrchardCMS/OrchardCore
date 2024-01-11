using OrchardCore.AuditTrail.Models;
using YesSql.Filters.Query;

namespace OrchardCore.AuditTrail.Services
{
    public class DefaultAuditTrailAdminListFilterParser : IAuditTrailAdminListFilterParser
    {
        private readonly IQueryParser<AuditTrailEvent> _parser;

        public DefaultAuditTrailAdminListFilterParser(IQueryParser<AuditTrailEvent> parser)
        {
            _parser = parser;
        }

        public QueryFilterResult<AuditTrailEvent> Parse(string text)
            => _parser.Parse(text);
    }
}
