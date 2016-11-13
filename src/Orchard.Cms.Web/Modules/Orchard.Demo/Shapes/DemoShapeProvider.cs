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
        public IHtmlContent Baz(string Text, int Count)
        {
            if (Count == 0)
                Count = 1;

            var sb = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                sb.Append(Text ?? "There is no text");
            }

            return new HtmlString(sb.ToString());
        }
    }
}