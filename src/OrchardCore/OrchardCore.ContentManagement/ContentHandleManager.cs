using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement
{
    public class ContentHandleManager : IContentHandleManager
    {
        private readonly IEnumerable<IContentHandleProvider> _contentHandleProviders;

        public ContentHandleManager(IEnumerable<IContentHandleProvider> contentHandleProviders)
        {
            _contentHandleProviders = contentHandleProviders.OrderBy(x => x.Order);
        }

        public async Task<string> GetContentItemIdAsync(string handle)
        {
            foreach (var provider in _contentHandleProviders)
            {
                var result = await provider.GetContentItemIdAsync(handle);

                if (!String.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            return null;
        }
    }
}
