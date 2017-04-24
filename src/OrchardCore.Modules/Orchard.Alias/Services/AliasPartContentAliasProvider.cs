using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.Alias.Indexes;
using YesSql;

namespace Orchard.Alias.Services
{
    public class AliasPartContentAliasProvider : IContentAliasProvider
    {
        private readonly ISession _session;

        public AliasPartContentAliasProvider(ISession session)
        {
            _session = session;
        }

        public int Order => 100;
        
        public async Task<string> GetContentItemIdAsync(string alias)
        {
            if (alias.StartsWith("alias:", System.StringComparison.OrdinalIgnoreCase))
            {
                alias = alias.Substring(6);

                var aliasPartIndex = await _session.QueryAsync<ContentItem, AliasPartIndex>(x => x.Alias == alias.ToLowerInvariant()).FirstOrDefault();
                return aliasPartIndex?.ContentItemId;
            }

            return null;
        }
    }
}
