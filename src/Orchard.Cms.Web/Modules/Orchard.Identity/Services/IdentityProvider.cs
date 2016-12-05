using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.Identity.Indexes;
using YesSql.Core.Services;

namespace Orchard.Identity.Services
{
    public class IdentityProvider : IContentIdentityProvider
    {
        private readonly ISession _session;

        public IdentityProvider(ISession session)
        {
            _session = session;
        }

        public async Task<ContentItem> LoadContentItemAsync(string key, string value)
        {

            if (key != "identifier")
            {
                return null;
            }
            else
            {
                return await _session.QueryAsync<ContentItem, IdentityPartIndex>(x => x.Identifier == value && x.Published == true && x.Latest == true).FirstOrDefault();
            }
        }
    }
}
