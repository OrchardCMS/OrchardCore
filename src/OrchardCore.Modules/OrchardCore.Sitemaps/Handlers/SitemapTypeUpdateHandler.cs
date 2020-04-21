using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Sitemaps.Handlers
{
    public class SitemapTypeUpdateHandler : ISitemapTypeUpdateHandler
    {
        private readonly IEnumerable<ISitemapSourceUpdateHandler> _sitemapSourceUpdateHandlers;

        public SitemapTypeUpdateHandler(IEnumerable<ISitemapSourceUpdateHandler> sitemapSourceUpdateHandlers)
        {
            _sitemapSourceUpdateHandlers = sitemapSourceUpdateHandlers;
        }

        public async Task UpdateSitemapAsync(SitemapUpdateContext context)
        {
            foreach (var sitemapSourceUpdateHandler in _sitemapSourceUpdateHandlers)
            {
                await sitemapSourceUpdateHandler.UpdateSitemapAsync(context);
            }
        }
    }
}
