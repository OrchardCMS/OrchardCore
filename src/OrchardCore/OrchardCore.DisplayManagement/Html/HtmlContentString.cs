using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using Cysharp.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OrchardCore.DisplayManagement.Html
{
    /// <summary>
    /// An optimization of <see cref="StringHtmlContent "/> that uses 'writer.Write(encoder.Encode(_value))', in place of using
    /// 'encoder.Encode(writer, _value)' that calls 'writer.Write' on each char if the string has even a single char to encode.
    /// </summary>
    [DebuggerDisplay("{DebuggerToString()}")]
    public class HtmlContentString : IHtmlContent
    {
        private readonly string _value;

        /// <summary>
        /// Creates a new instance of <see cref="HtmlContentString"/>
        /// </summary>
        /// <param name="value"><see cref="string"/> to be HTML encoded when <see cref="WriteTo"/> is called.</param>
        public HtmlContentString(string value) => _value = value;

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder) => writer.Write(encoder.Encode(_value));

        private string DebuggerToString()
        {
            var writer = new ZStringWriter();
            WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
    }
}
