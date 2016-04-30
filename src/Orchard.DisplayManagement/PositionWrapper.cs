using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Text.Encodings.Web;

namespace Orchard.DisplayManagement
{
    public class PositionWrapper : IHtmlContent, IPositioned
    {
        private IHtmlContent _value;
        public string Position { get; set; }

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