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

        public Task<string> GetContentItemIdAsync(string alias)
        {
            if (alias.StartsWith("slug:", System.StringComparison.OrdinalIgnoreCase))
            {
                alias = alias.Substring(5);

                if (!alias.StartsWith('/'))
                {
                    alias = "/" + alias;
                }

                if (_autorouteEntries.TryGetEntryByPath(alias, out var entry))
                {
                    // TODO this requires more work, and interface changes to support contained content items.
                    // as it will require returning the id and jsonPath.
                    return Task.FromResult(entry.ContentItemId);
                }
            }

            return Task.FromResult<string>(null);
        }
    }
}
