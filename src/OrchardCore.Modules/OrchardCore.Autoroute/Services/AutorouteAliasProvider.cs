using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteAliasProvider : IContentAliasProvider
    {
        private readonly IAutorouteEntries _autorouteEntries;

        public AutorouteAliasProvider(IAutorouteEntries autorouteEntries)
        {
            _autorouteEntries = autorouteEntries;
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

                (var found, var entry) = await _autorouteEntries.TryGetEntryByPathAsync(alias);

                if (found)
                {
                    // TODO this requires more work, and interface changes to support contained content items.
                    // as it will require returning the id and jsonPath.
                    return entry.ContentItemId;
                }
            }

            return null;
        }
    }
}
