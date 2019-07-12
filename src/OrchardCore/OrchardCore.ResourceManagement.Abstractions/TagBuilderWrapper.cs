using System;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.ResourceManagement
{
    /// <summary>
    /// A custom <see cref="IHtmlContent"/> wrapping a <see cref="TagBuilder"/>
    /// so that any 'integrity' attribute is not html encoded.
    /// </summary>
    internal class TagBuilderWrapper : IHtmlContent
    {
        private readonly TagBuilder _tagBuilder;

        public TagBuilderWrapper(TagBuilder tagBuider)
        {
            _tagBuilder = tagBuider;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            switch (_tagBuilder.TagRenderMode)
            {
                case TagRenderMode.StartTag:
                    writer.Write("<");
                    writer.Write(_tagBuilder.TagName);
                    AppendAttributes(writer, encoder);
                    writer.Write(">");
                    break;
                case TagRenderMode.EndTag:
                    writer.Write("</");
                    writer.Write(_tagBuilder.TagName);
                    writer.Write(">");
                    break;
                case TagRenderMode.SelfClosing:
                    writer.Write("<");
                    writer.Write(_tagBuilder.TagName);
                    AppendAttributes(writer, encoder);
                    writer.Write(" />");
                    break;
                default:
                    writer.Write("<");
                    writer.Write(_tagBuilder.TagName);
                    AppendAttributes(writer, encoder);
                    writer.Write(">");
                    if (_tagBuilder.HasInnerHtml)
                    {
                        _tagBuilder.InnerHtml.WriteTo(writer, encoder);
                    }
                    writer.Write("</");
                    writer.Write(_tagBuilder.TagName);
                    writer.Write(">");
                    break;
            }
        }

        private void AppendAttributes(TextWriter writer, HtmlEncoder encoder)
        {
            foreach (var attribute in _tagBuilder.Attributes)
            {
                var key = attribute.Key;
                if (string.Equals(key, "id", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrEmpty(attribute.Value))
                {
                    continue;
                }

                writer.Write(" ");
                writer.Write(key);
                writer.Write("=\"");
                if (attribute.Value != null)
                {
                    if (attribute.Key == "integrity")
                    {
                        writer.Write(attribute.Value);
                    }
                    else
                    {
                        encoder.Encode(writer, attribute.Value);
                    }
                }

                writer.Write("\"");
            }
        }
    }
}
