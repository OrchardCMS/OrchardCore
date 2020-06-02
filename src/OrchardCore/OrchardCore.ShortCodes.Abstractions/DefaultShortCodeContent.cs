using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.ShortCodes
{
    public class DefaultShortCodeContent : ShortCodeContent
    {
        private object _singleContent;
        private bool _isSingleContentSet;
        private bool _isModified;
        private bool _hasContent;
        private List<object> _buffer;

        public override bool IsModified => _isModified;

        private List<object> Buffer
        {
            get
            {
                if (_buffer == null)
                {
                    _buffer = new List<object>();
                }

                if (_isSingleContentSet)
                {
                    _buffer.Add(_singleContent);
                    _isSingleContentSet = false;
                }

                return _buffer;
            }
        }

        public override ShortCodeContent Append(string unencoded) => AppendCore(unencoded);

        public override ShortCodeContent AppendHtml(IHtmlContent content) => AppendCore(content);

        public override ShortCodeContent AppendHtml(string encoded)
        {
            if (encoded == null)
            {
                return AppendCore(null);
            }

            return AppendCore(new HtmlString(encoded));
        }

        public override ShortCodeContent Clear()
        {
            _hasContent = false;
            _isModified = true;
            _isSingleContentSet = false;
            _buffer?.Clear();

            return this;
        }

        public override void CopyTo(IHtmlContentBuilder builder)
        {
            throw new NotImplementedException();
        }

        public override string GetContent() => GetContent(HtmlEncoder.Default);

        public override string GetContent(HtmlEncoder encoder)
        {
            if (!_hasContent)
            {
                return string.Empty;
            }

            using (var writer = new StringWriter())
            {
                WriteTo(writer, encoder);

                return writer.ToString();
            }
        }

        public override void MoveTo(IHtmlContentBuilder builder)
        {
            throw new NotImplementedException();
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (!_hasContent)
            {
                return;
            }

            if (_isSingleContentSet)
            {
                WriteToCore(_singleContent, writer, encoder);

                return;
            }

            for (var i = 0; i < (_buffer?.Count ?? 0); i++)
            {
                WriteToCore(Buffer[i], writer, encoder);
            }
        }

        private ShortCodeContent AppendCore(object entry)
        {
            if (!_hasContent)
            {
                _isSingleContentSet = true;
                _singleContent = entry;
            }
            else
            {
                Buffer.Add(entry);
            }

            _isModified = true;
            _hasContent = true;

            return this;
        }

        private void WriteToCore(object entry, TextWriter writer, HtmlEncoder encoder)
        {
            if (entry == null)
            {
                return;
            }

            if (entry is string stringValue)
            {
                encoder.Encode(writer, stringValue);
            }
            else
            {
                ((IHtmlContent)entry).WriteTo(writer, encoder);
            }
        }
    }
}
