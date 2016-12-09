using System.Threading.Tasks;
using Orchard.ContentManagement;

namespace Orchard.Autoroute.Services
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
            string contentItemId;
            
            if (_autorouteEntries.TryGetContentItemId(alias, out contentItemId))
            {
                return Task.FromResult(contentItemId);
            }

            return Task.FromResult<string>(null);
        }
    }
}
