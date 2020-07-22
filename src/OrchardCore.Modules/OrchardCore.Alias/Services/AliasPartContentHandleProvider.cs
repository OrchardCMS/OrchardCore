using System.Threading.Tasks;
using OrchardCore.Alias.Indexes;
using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Alias.Services
{
    public class AliasPartContentHandleProvider : IContentHandleProvider
    {
        private readonly ISession _session;

        public AliasPartContentHandleProvider(ISession session)
        {
            _session = session;
        }

        public int Order => 100;

        public async Task<string> GetContentItemIdAsync(string handle)
        {
            if (handle.StartsWith("alias:", System.StringComparison.OrdinalIgnoreCase))
            {
                handle = handle.Substring(6);

                var aliasPartIndex = await _session.Query<ContentItem, AliasPartIndex>(x => x.Alias == handle.ToLowerInvariant()).FirstOrDefaultAsync();
                return aliasPartIndex?.ContentItemId;
            }

            return null;
        }
    }
}
