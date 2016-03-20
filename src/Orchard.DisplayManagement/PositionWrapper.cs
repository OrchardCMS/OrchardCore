using Microsoft.AspNet.Html;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.WebEncoders;
using System.IO;

namespace Orchard.DisplayManagement
{
    public class PositionWrapper : IHtmlContent, IPositioned
    {
        private IHtmlContent _value;
        public string Position { get; private set; }

        public PositionWrapper(IHtmlContent value, string position)
        {
            _value = value;
            Position = position;
        }

        public PositionWrapper(string value, string position)
        {
            _value = new HtmlString(HtmlEncoder.Default.HtmlEncode(value));
            Position = position;
        }

        public void WriteTo(TextWriter writer, IHtmlEncoder encoder)
        {
            _value.WriteTo(writer, encoder);
        }
    }
}