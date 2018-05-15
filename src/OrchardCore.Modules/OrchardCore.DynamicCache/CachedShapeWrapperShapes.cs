using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DynamicCache
{
    public class CachedShapeWrapperShapes : IShapeAttributeProvider
    {
        [Shape]
        public IHtmlContent CachedShapeWrapper(dynamic Shape)
        {
            var sb = new StringBuilder();
            var metadata = (ShapeMetadata) Shape.Metadata;
            var cache = metadata.Cache();
            
            sb.AppendLine($"<!-- CACHED SHAPE: {cache.CacheId} ({Guid.NewGuid()})");
            sb.AppendLine($"          VARY BY: {String.Join(", ", cache.Contexts)}");
            sb.AppendLine($"     DEPENDENCIES: {String.Join(", ", cache.Tags)}");
            sb.AppendLine($"       EXPIRES ON: {cache.ExpiresOn}");
            sb.AppendLine($"    EXPIRES AFTER: {cache.ExpiresAfter}");
            sb.AppendLine($"  EXPIRES SLIDING: {cache.ExpiresSliding}");
            sb.AppendLine("-->");
            
            using (var sw = new StringWriter())
            {
                metadata.ChildContent.WriteTo(sw, HtmlEncoder.Default);
                sb.AppendLine(sw.ToString());
            }
            
            sb.AppendLine($"<!-- END CACHED SHAPE: {cache.CacheId} -->");

            return new HtmlString(sb.ToString());
        }
    }
}
