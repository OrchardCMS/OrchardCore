using System;
using System.Text;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class CacheBlockShapes : IShapeAttributeProvider
    {
        [Shape]
        public IHtmlContent CacheBlock(string ChildContent)
        {
            return new HtmlString(ChildContent);
        }

        [Shape]
        public IHtmlContent CacheBlockWrapper(dynamic Shape)
        {
            var sb = new StringBuilder();
            var metadata = (ShapeMetadata) Shape.Metadata;
            var cache = metadata.Cache();

            sb.AppendLine($"<!-- CACHE BLOCK: {cache.CacheId} ({Guid.NewGuid()})");
            sb.AppendLine($"        CONTEXTS: {String.Join(", ", cache.Contexts)}");
            sb.AppendLine($"            TAGS: {String.Join(", ", cache.Tags)}");
            sb.AppendLine($"          DURING: {cache.Duration}");
            sb.AppendLine($"         SLIDING: {cache.SlidingExpirationWindow}");
            sb.AppendLine("-->");

            sb.AppendLine(metadata.ChildContent.ToString());
            
            sb.AppendLine($"<!-- END CACHE BLOCK: {cache.CacheId} -->");

            return new HtmlString(sb.ToString());
        }
    }
}
