using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.WebEncoders;

namespace Orchard.DisplayManagement.Descriptors {
    public class FooShapeProvider : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Foo")
                .OnDisplaying(displaying =>
                    displaying.ChildContent = new HtmlString("<h1>Hi</h1>"))
                .BoundAs("Foo", shapeDescriptor => displayContext => {
                    var shape = (IShape)displayContext.Value;
                    return shape.Metadata.ChildContent ?? new HtmlString("");
                });
        }
    }
}
