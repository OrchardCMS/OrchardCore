using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.ShortCodes
{
    public class ShortCodeOutput : IHtmlContent
    {
        private readonly Func<HtmlEncoder, Task<ShortCodeContent>> _getChildContentAsync;
        private ShortCodeContent _content;
        private bool _wasSuppressOutputCalled;

        public ShortCodeOutput(string shortCodeName)
            : this(
                  shortCodeName,
                  encoder => Task.FromResult<ShortCodeContent>(new DefaultShortCodeContent()))
        {

        }

        public ShortCodeOutput(string shortCodeName, Func<HtmlEncoder, Task<ShortCodeContent>> getChildContentAsync)
        {
            ShortCodeName = shortCodeName;
            _getChildContentAsync = getChildContentAsync;
        }

        private string StartShortCode => $"[{ShortCodeName}]";

        private string EndShortCode => $"[/{ShortCodeName}]";

        public string ShortCodeName { get; set; }

        public bool IsContentModified => _wasSuppressOutputCalled || _content?.IsModified == true;

        public ShortCodeContent Content
        {
            get
            {
                if (_content == null)
                {
                    _content = new DefaultShortCodeContent();
                }

                return _content;
            }
            set
            {
                _content = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public Task<ShortCodeContent> GetChildContentAsync(HtmlEncoder encoder) => _getChildContentAsync(encoder);

        public async Task<ShortCodeContent> GetChildContentAsync()
        {
            var childContent = await GetChildContentAsync(HtmlEncoder.Default);
            var content = ExtractShortCodeContent(childContent.GetContent());
            var shortCodeContent = new DefaultShortCodeContent();
            shortCodeContent.SetHtmlContent(content.Content);

            return shortCodeContent;
        }

        public void SuppressOutput()
        {
            ShortCodeName = null;
            _wasSuppressOutputCalled = true;
            _content?.Clear();
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            var childContent = GetChildContentAsync(HtmlEncoder.Default).GetAwaiter().GetResult();
            var markup = childContent.GetContent();
            var content = ExtractShortCodeContent(markup);
            if (content.Span == ShortCodeSpan.Default)
            {
                writer.Write(content.Content);
            }
            else
            {
                var before = markup.Substring(0, content.Span.Start - StartShortCode.Length);
                var after = markup.Substring(content.Span.End + EndShortCode.Length);

                writer.Write(before + Content.GetContent() + after);
            }
        }

        private (string Content, ShortCodeSpan Span) ExtractShortCodeContent(string markup)
        {
            var startShortCodeIndex = markup.IndexOf(StartShortCode) + StartShortCode.Length;
            var endShortCodeIndex = markup.IndexOf(EndShortCode);

            if (startShortCodeIndex == -1 || endShortCodeIndex == -1)
            {
                return (markup, ShortCodeSpan.Default);
            }

            var content = markup[startShortCodeIndex..endShortCodeIndex];

            return (content, new ShortCodeSpan(startShortCodeIndex, endShortCodeIndex));
        }
    }
}
