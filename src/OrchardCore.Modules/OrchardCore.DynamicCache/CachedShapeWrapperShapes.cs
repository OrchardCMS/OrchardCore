using System;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DynamicCache
{
    public class CachedShapeWrapperShapes : IShapeAttributeProvider
    {
        [Shape]
        public IHtmlContent CachedShapeWrapper(IShape Shape)
        {
            // No need to optimize this code as it will be used for debugging purpose

            var contentBuilder = new HtmlContentBuilder();
            var metadata = Shape.Metadata;
            var cache = metadata.Cache();

            contentBuilder.AppendHtml($"<!-- CACHED SHAPE: {cache.CacheId} ({Guid.NewGuid()})");
            contentBuilder.AppendHtml($"          VARY BY: {String.Join(", ", cache.Contexts)}");
            contentBuilder.AppendHtml($"     DEPENDENCIES: {String.Join(", ", cache.Tags)}");
            contentBuilder.AppendHtml($"       EXPIRES ON: {cache.ExpiresOn}");
            contentBuilder.AppendHtml($"    EXPIRES AFTER: {cache.ExpiresAfter}");
            contentBuilder.AppendHtml($"  EXPIRES SLIDING: {cache.ExpiresSliding}");
            contentBuilder.AppendHtml("-->");

            contentBuilder.AppendHtml(metadata.ChildContent);

            contentBuilder.AppendHtml($"<!-- END CACHED SHAPE: {cache.CacheId} -->");

            return contentBuilder;
        }
    }
}
