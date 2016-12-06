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

        public Task<ContentItem> GetAsync(string key, string value)
        {

            if (key != "identifier")
            {
                return Task.FromResult<ContentItem>(null);
            }
            else
            {
                return _session.QueryAsync<ContentItem, IdentityPartIndex>(x => x.Identifier == value && x.Published == true && x.Latest == true).FirstOrDefault();
            }
        }
    }
}
