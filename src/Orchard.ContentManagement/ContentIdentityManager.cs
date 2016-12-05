using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<ContentItem> LoadContentItemAsync(string key, string value)
        {
            foreach(var provider in _providers)
            {
                var contentItem = await provider.LoadContentItemAsync(key, value);
                if (contentItem != null)
                {
                    return contentItem;
                }
            }

            return null;
        }
    }
}
