using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement
{
    public class ContentAliasManager : IContentAliasManager
    {
        private readonly IEnumerable<IContentAliasProvider> _contentAliasProviders;

        public ContentAliasManager(IEnumerable<IContentAliasProvider> contentAliasProviders)
        {
            _contentAliasProviders = contentAliasProviders.OrderBy(x => x.Order);
        }

        public async Task<string> GetContentItemIdAsync(string alias)
        {
            foreach (var provider in _contentAliasProviders)
            {
                var result = await provider.GetContentItemIdAsync(alias);

                if (!String.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            return null;
        }
    }
}
