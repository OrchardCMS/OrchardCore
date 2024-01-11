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

        public override Task PublishedAsync(PublishContentContext context)
        {
            var updateContext = new SitemapUpdateContext
            {
                UpdateObject = context.ContentItem,
            };

            return _sitemapUpdateHandler.UpdateSitemapAsync(updateContext);
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            var updateContext = new SitemapUpdateContext
            {
                UpdateObject = context.ContentItem,
            };

            return _sitemapUpdateHandler.UpdateSitemapAsync(updateContext);
        }
    }
}
