using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Sitemaps.Handlers
{
    public class DefaultSitemapUpdateHandler : ISitemapUpdateHandler
    {
        private readonly IEnumerable<ISitemapTypeUpdateHandler> _sitemapTypeUpdateHandlers;

        public DefaultSitemapUpdateHandler(IEnumerable<ISitemapTypeUpdateHandler> sitemapTypeUpdateHandlers)
        {
            _sitemapTypeUpdateHandlers = sitemapTypeUpdateHandlers;
        }

        public async Task UpdateSitemapAsync(SitemapUpdateContext context)
        {
            foreach (var sitemapTypeUpdateHandler in _sitemapTypeUpdateHandlers)
            {
                await sitemapTypeUpdateHandler.UpdateSitemapAsync(context);
            }
        }
    }
}
