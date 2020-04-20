using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Sitemaps.Handlers;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapUpdateHandler : ContentHandlerBase
    {
        private readonly ISitemapUpdateHandler _sitemapUpdateHandler;

        public ContentTypesSitemapUpdateHandler(ISitemapUpdateHandler sitemapUpdateHandler)
        {
            _sitemapUpdateHandler = sitemapUpdateHandler;
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            var updateContext = new SitemapUpdateContext
            {
                UpdatedObject = context.ContentItem,
            };

            await _sitemapUpdateHandler.UpdateSitemapAsync(updateContext);
        }

        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            var updateContext = new SitemapUpdateContext
            {
                UpdatedObject = context.ContentItem,
            };

            await _sitemapUpdateHandler.UpdateSitemapAsync(updateContext);
        }
    }
}
