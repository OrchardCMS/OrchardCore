using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.ShortCodes
{
    public class ShortCodeOutput : IHtmlContent
    {
        private static IEnumerable<string> _shortCodeTargets;

        private readonly Func<HtmlEncoder, Task<ShortCodeContent>> _getChildContentAsync;

        private ShortCodeContent _content;
        private bool _wasSuppressOutputCalled;
        private string _selectedShortCode;
        private (string Content, ShortCodeSpan Span) _selectedShortCodeContent;

        public ShortCodeOutput(IShortCode shortCode)
            : this(
                  shortCode,
                  encoder => Task.FromResult<ShortCodeContent>(new DefaultShortCodeContent()))
        {

        }

        public ShortCodeOutput(IShortCode shortCode, Func<HtmlEncoder, Task<ShortCodeContent>> getChildContentAsync)
        {
            ShortCode = shortCode;
            _getChildContentAsync = getChildContentAsync;
        }

        public IShortCode ShortCode { get; }

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
            _selectedShortCode = null;
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

            writer.Write(Content.GetContent());
        }

        private (string Content, ShortCodeSpan Span) ExtractShortCodeContent(string markup)
        {
            if (_selectedShortCodeContent.Content != null && !_selectedShortCodeContent.Span.Equals(ShortCodeSpan.Default))
            {
                return _selectedShortCodeContent;
            }

            foreach (var shortCodeName in GetShortCodeTargets())
            {
                var startShortCode = $"[{shortCodeName}]";
                var endShortCode = $"[/{shortCodeName}]";
                var startShortCodeIndex = markup.IndexOf(startShortCode) + startShortCode.Length;
                var endShortCodeIndex = markup.IndexOf(endShortCode);

                if (startShortCodeIndex == -1 || endShortCodeIndex == -1)
                {
                    continue;
                }

                _selectedShortCode = shortCodeName;

                var content = markup[startShortCodeIndex..endShortCodeIndex];
                _selectedShortCodeContent = (content, new ShortCodeSpan(startShortCodeIndex, endShortCodeIndex));

                return _selectedShortCodeContent;
            }

            return (markup, ShortCodeSpan.Default);
        }

        private IEnumerable<string> GetShortCodeTargets()
        {
            if (_shortCodeTargets != null)
            {
                return _shortCodeTargets;
            }

            _shortCodeTargets = Enumerable.Empty<string>();
            var shortCodeTargetAttributes = ShortCode.GetType()
                .GetCustomAttributes(typeof(ShortCodeTargetAttribute), false);
            if (shortCodeTargetAttributes.Length > 0)
            {
                _shortCodeTargets = shortCodeTargetAttributes.OfType<ShortCodeTargetAttribute>().Select(a => a.Name);
            }

            return _shortCodeTargets;
        }
    }
}
