using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement
{
    public class PositionWrapper : IHtmlContent, IPositioned, IShape
    {
        private readonly IHtmlContent _value;
        public string Position { get; set; }

        public ShapeMetadata Metadata { get; set; } = new ShapeMetadata();

        public string Id { get; set; }

        public string TagName { get; set; }

        public IList<string> Classes { get; }

        public IDictionary<string, string> Attributes { get; }

        private Dictionary<string, object> _properties;

        public IDictionary<string, object> Properties => _properties ??= new Dictionary<string, object>();

        public IReadOnlyList<IPositioned> Items => throw new System.NotImplementedException();

        public PositionWrapper(IHtmlContent value, string position)
        {
            _value = value;
            Position = position;
        }

        public PositionWrapper(string value, string position)
        {
            _value = new HtmlContentString(value);
            Position = position;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _value.WriteTo(writer, encoder);
        }

        public ValueTask<IShape> AddAsync(object item, string position)
        {
            throw new System.NotImplementedException();
        }
    }
}
