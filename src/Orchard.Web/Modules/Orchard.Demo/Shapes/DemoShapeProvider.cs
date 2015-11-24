using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.WebEncoders;

namespace Orchard.DisplayManagement.Descriptors
{
    public class DemoShapeProvider : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Foo")
                .OnDisplaying(displaying =>
                    displaying.ChildContent = new HtmlString("<h1>Hi</h1>")
                );
        }

        [Shape]
        public IHtmlContent Baz(string text)
        {
            return new HtmlString($"<em>{text}</em>");
        }
    }
}
