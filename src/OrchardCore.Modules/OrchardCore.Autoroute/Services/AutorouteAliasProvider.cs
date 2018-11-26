using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagment.Routable;

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

                if (!alias.StartsWith("/"))
                {
                    alias = "/" + alias;
                }

                if (_autorouteEntries.TryGetAutorouteEntryByPath(alias, out var entry))
                {
                    return Task.FromResult(entry.ActualContentItemId);
                }
            }

            return Task.FromResult<string>(null);
        }
    }
}
