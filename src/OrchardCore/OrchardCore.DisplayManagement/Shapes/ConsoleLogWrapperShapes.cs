using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Shapes
{
    [Feature(Application.DefaultFeatureId)]
    public class ConsoleLogWrapperShapes : IShapeAttributeProvider
    {
        const string FormatConsole = "<script>console.log({0})</script>";

        [Shape]
        public IHtmlContent ShapeConsoleLogWrapper(dynamic Shape)
        {
            var iShape = (IShape)Shape;

            var sb = new StringBuilder();
            var metadata = (ShapeMetadata)Shape.Metadata;

            sb.Append(string.Format(FormatConsole, iShape.ShapeToJson().ToString()));
            using (var sw = new StringWriter())
            {
                metadata.ChildContent.WriteTo(sw, HtmlEncoder.Default);
                sb.AppendLine(sw.ToString());
            }

            return new HtmlString(sb.ToString());
        }
    }
}
