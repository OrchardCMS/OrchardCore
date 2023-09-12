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
#pragma warning disable CA1822 // Mark members as static
        public IHtmlContent CachedShapeWrapper(IShape Shape)
#pragma warning restore CA1822 // Mark members as static
        {
            // No need to optimize this code as it will be used for debugging purpose

            var contentBuilder = new HtmlContentBuilder();
            var metadata = Shape.Metadata;
            var cache = metadata.Cache();

            contentBuilder.AppendLine();
            contentBuilder.AppendHtmlLine($"<!-- CACHED SHAPE: {cache.CacheId} ({Guid.NewGuid()})");
            contentBuilder.AppendHtmlLine($"          VARY BY: {String.Join(", ", cache.Contexts)}");
            contentBuilder.AppendHtmlLine($"     DEPENDENCIES: {String.Join(", ", cache.Tags)}");
            contentBuilder.AppendHtmlLine($"       EXPIRES ON: {cache.ExpiresOn}");
            contentBuilder.AppendHtmlLine($"    EXPIRES AFTER: {cache.ExpiresAfter}");
            contentBuilder.AppendHtmlLine($"  EXPIRES SLIDING: {cache.ExpiresSliding}");
            contentBuilder.AppendHtmlLine("-->");

            contentBuilder.AppendHtml(metadata.ChildContent);

            contentBuilder.AppendLine();
            contentBuilder.AppendHtmlLine($"<!-- END CACHED SHAPE: {cache.CacheId} -->");

            return contentBuilder;
        }
    }
}
