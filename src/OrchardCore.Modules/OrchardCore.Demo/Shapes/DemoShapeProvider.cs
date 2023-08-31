using System.Text;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Descriptors
{
    public class DemoShapeProvider : IShapeTableProvider, IShapeAttributeProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Foo")
                .OnDisplaying(displaying =>
                    displaying.ChildContent = new HtmlString("<h1>Hi</h1>")
                );
        }

        [Shape]
#pragma warning disable CA1822 // Mark members as static
        public IHtmlContent Baz(string text, int count)
#pragma warning restore CA1822 // Mark members as static
        {
            var sb = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                sb.Append(text);
            }

            return new HtmlString(sb.ToString());
        }
    }
}
