using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement
{
    public class PositionWrapper : IHtmlContent, IPositioned, IShape
    {
        private IHtmlContent _value;
        public string Position { get; set; }

        public ShapeMetadata Metadata { get; set; } = new ShapeMetadata();

        public string Id { get; set; }

        public string TagName { get; set; }

        public IList<string> Classes { get; }

        public IDictionary<string, string> Attributes { get; }

        private Dictionary<string, object> _properties;
        public IDictionary<string, object> Properties => _properties = _properties ?? new Dictionary<string, object>();

        public PositionWrapper(IHtmlContent value, string position)
        {
            _value = value;
            Position = position;
        }

        public PositionWrapper(string value, string position)
        {
            _value = new StringHtmlContent(value);
            Position = position;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _value.WriteTo(writer, encoder);
        }
    }
}
