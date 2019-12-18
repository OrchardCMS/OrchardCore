using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Shapes
{
    [Feature(Application.DefaultFeatureId)]
    public class ConsoleLogWrapperShapes : IShapeAttributeProvider
    {
        [Shape]
        public IHtmlContent ShapeConsoleLogWrapper(IShape Shape)
        {
            var contentBuilder = new HtmlContentBuilder(3);
            var metadata = Shape.Metadata;

            contentBuilder.AppendHtml("<script>console.log(");
            contentBuilder.AppendHtml(Shape.ShapeToJson().ToString());
            contentBuilder.AppendHtml(")</script>");

            contentBuilder.AppendHtml(metadata.ChildContent);

            return contentBuilder;
        }
    }
}
