using Microsoft.AspNetCore.Html;
using System.Text;

namespace Orchard.DisplayManagement.Descriptors
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
        public IHtmlContent Baz(string text, int count)
        {
            if (count == 0)
                count = 1;

            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(text ?? "There is no text");
            }

            return new HtmlString(sb.ToString());
        }
    }
}