using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement
{
	public class PositionWrapper : IHtmlContent, IPositioned, IShape
    {
        private IHtmlContent _value;
        public string Position { get; set; }

		public ShapeMetadata Metadata { get; set; } = new ShapeMetadata();

		public string Id { get; set; }

		public IList<string> Classes { get; }

		public IDictionary<string, string> Attributes { get; }

		public PositionWrapper(IHtmlContent value, string position)
        {
            _value = value;
            Position = position;
        }

        public PositionWrapper(string value, string position)
        {
            _value = new HtmlString(HtmlEncoder.Default.Encode(value));
            Position = position;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _value.WriteTo(writer, encoder);
        }
    }
}