using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentManagement.Routing;
using YesSql;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteAliasProvider : IContentAliasProvider
    {
        private readonly IAutorouteEntries _autorouteEntries;
        private readonly ISession _session;

        public AutorouteAliasProvider(IAutorouteEntries autorouteEntries, ISession session)
        {
            _autorouteEntries = autorouteEntries;
            _session = session;
        }

        public int Order => 10;

        public async Task<string> GetContentItemIdAsync(string alias)
        {
            if (alias.StartsWith("slug:", System.StringComparison.OrdinalIgnoreCase))
            {
                alias = alias.Substring(5);

                if (!alias.StartsWith('/'))
                {
                    alias = "/" + alias;
                }

                // This only contains published items.
                if (_autorouteEntries.TryGetContentItemId(alias, out var contentItemId))
                {
                    return contentItemId;
                } else // Not relevant to check against published here as we are only querying for the content item id for the path / alias provided.
                {
                    var autoroutePartIndex = await _session.Query<ContentItem, AutoroutePartIndex>(x => x.Path == alias.ToLowerInvariant()).FirstOrDefaultAsync();
                    return autoroutePartIndex?.ContentItemId;
                }
            }

            return null;
        }
    }
}
