using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.ShortCodes
{
    public abstract class ShortCodeContent : IHtmlContentBuilder
    {
        public abstract bool IsModified { get; }

        public abstract ShortCodeContent Append(string unencoded);

        public abstract ShortCodeContent AppendHtml(string encoded);

        public abstract ShortCodeContent AppendHtml(IHtmlContent htmlContent);

        public abstract ShortCodeContent Clear();

        public abstract void CopyTo(IHtmlContentBuilder builder);

        public abstract string GetContent();

        public abstract string GetContent(HtmlEncoder encoder);

        public abstract void MoveTo(IHtmlContentBuilder builder);

        public ShortCodeContent SetContent(string unencoded)
        {
            HtmlContentBuilderExtensions.SetContent(this, unencoded);

            return this;
        }

        public ShortCodeContent SetHtmlContent(IHtmlContent htmlContent)
        {
            HtmlContentBuilderExtensions.SetHtmlContent(this, htmlContent);

            return this;
        }

        public ShortCodeContent SetHtmlContent(string encoded)
        {
            HtmlContentBuilderExtensions.SetHtmlContent(this, encoded);

            return this;
        }

        public abstract void WriteTo(TextWriter writer, HtmlEncoder encoder);

        IHtmlContentBuilder IHtmlContentBuilder.Append(string unencoded) => Append(unencoded);

        IHtmlContentBuilder IHtmlContentBuilder.AppendHtml(IHtmlContent content) => AppendHtml(content);

        IHtmlContentBuilder IHtmlContentBuilder.AppendHtml(string encoded) => AppendHtml(encoded);

        IHtmlContentBuilder IHtmlContentBuilder.Clear() => Clear();
    }
}
