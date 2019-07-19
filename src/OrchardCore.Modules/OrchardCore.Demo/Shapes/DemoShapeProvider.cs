using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Descriptors
{
    public class DemoShapeProvider : IShapeTableProvider, IShapeAttributeProvider
    {
        public Task DiscoverAsync(ShapeTableBuilder builder)
        {
            builder.Describe("Foo")
                .OnDisplaying(displaying =>
                    displaying.ChildContent = new HtmlString("<h1>Hi</h1>")
                );

            return Task.CompletedTask;
        }

        [Shape]
        public IHtmlContent Baz(string text, int count)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(text);
            }

            return new HtmlString(sb.ToString());
        }
    }
}
