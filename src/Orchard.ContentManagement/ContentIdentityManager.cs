using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.ContentManagement
{
    public class ContentIdentityManager : IContentIdentityManager
    {
        private readonly IEnumerable<IContentIdentityProvider> _providers;

        public ContentIdentityManager(IEnumerable<IContentIdentityProvider> providers)
        {
            _providers = providers;
        }

        public async Task<ContentItem> GetAsync(ContentIdentity identity)
        {
            foreach(var provider in _providers)
            {
                foreach(var name in identity.Names)
                {
                    var contentItem = await provider.GetAsync(name, identity.Get(name));

                    if (contentItem != null)
                    {
                        return contentItem;
                    }
                }
            }

            return null;
        }
    }
}
