using System.Threading.Tasks;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentManagement.Routing;
using YesSql;

namespace OrchardCore.Autoroute.Routing
{
    public class AutorouteContentRouteValidationProvider : IContentRouteValidationProvider
    {
        private readonly ISession _session;

        public AutorouteContentRouteValidationProvider(ISession session)
        {
            _session = session;
        }

        public async Task<bool> IsPathUniqueAsync(string path, string contentItemId)
        {
            return (await _session.QueryIndex<AutoroutePartIndex>(o => o.ContentItemId != contentItemId && o.Path == path).CountAsync()) == 0;
        }
    }
}
