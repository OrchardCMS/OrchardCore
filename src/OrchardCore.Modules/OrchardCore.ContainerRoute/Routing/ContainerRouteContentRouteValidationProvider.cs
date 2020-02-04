using System.Threading.Tasks;
using OrchardCore.ContainerRoute.Indexes;
using OrchardCore.ContentManagement.Routing;
using YesSql;

namespace OrchardCore.ContainerRoute.Routing
{
    public class ContainerRouteContentRouteValidationProvider : IContentRouteValidationProvider
    {
        private readonly ISession _session;

        public ContainerRouteContentRouteValidationProvider(ISession session)
        {
            _session = session;
        }

        public async Task<bool> IsPathUniqueAsync(string path, string contentItemId)
        {
            return (await _session.QueryIndex<ContainerRoutePartIndex>(c => (c.ContainedContentItemId != contentItemId || c.ContainerContentItemId != contentItemId) && c.Path == path).CountAsync()) == 0;
        }
    }
}
