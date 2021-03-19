using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Liquid
{
    /// <summary>
    /// An <see cref="IHtmlContent"/> implementation that inherits from <see cref="TextWriter"/> to write to the ASP.NET ViewBufferTextWriter
    /// in an optimal way. The ViewBufferTextWriter implementation in ASP.NET won't allocate if Write(object) is invoked with instances of
    /// <see cref="IHtmlContent"/>.
    /// </summary>
    public class ViewBufferTextWriterContent : TextWriter, IHtmlContent
    {
        private static readonly HtmlString[] _internedChars = InitInternedChars();
        private const int _internedCharsLength = 256;

        private readonly List<IHtmlContent> _fragments = new List<IHtmlContent>();

        private static HtmlString[] InitInternedChars()
        {
            // Memoize all ASCII chars to prevent allocations
            var internedChars = new HtmlString[_internedCharsLength];

            for (var i = 0; i < _internedCharsLength; i++)
            {
                internedChars[i] = new HtmlString(((char)i).ToString());
            }

            return internedChars;
        }

        public override Encoding Encoding => Encoding.UTF8;

        // Invoked when used as TextWriter to intercept what is supposed to be written
        public override void Write(string value)
        {
            _fragments.Add(new HtmlString(value));
        }

        public override void Write(char value)
        {
            // perf: when a string is encoded (e.g. {{ value }} in a view) and the content contains some encoded chars,
            // the TextWriter implementation will call Write(char) for the whole string, creating as many fragments as chars in the string.
            // This could be optimized by creating a custom HTML encoder that finds blocks
            // https://source.dot.net/#System.Text.Encodings.Web/System/Text/Encodings/Web/TextEncoder.cs,365

            if (value < _internedCharsLength)
            {
                _fragments.Add(_internedChars[value]);
            }
            else
            {
                _fragments.Add(new HtmlString(value.ToString()));
            }
        }

        public override void Write(char[] buffer)
        {
            _fragments.Add(new CharrArrayHtmlContent(buffer));
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (index == 0 && buffer.Length == count)
            {
                _fragments.Add(new CharrArrayHtmlContent(buffer));
            }
            else
            {
                _fragments.Add(new CharrArrayFragmentHtmlContent(buffer, index, count));
            }
        }

        public override void Write(ReadOnlySpan<char> buffer)
        {
            _fragments.Add(new CharrArrayHtmlContent(buffer.ToArray()));
        }

        // Invoked by IHtmlContent when rendered on the final output
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            foreach (var fragment in _fragments)
            {
                writer.Write(fragment);
            }
        }

        public override Task FlushAsync()
        {
            // Override since the base implementation does unnecessary work
            return Task.CompletedTask;
        }

        /// <summary>
        /// An <see cref="IHtmlContent"/> implementation that wraps an HTML encoded <see langword="char[]"/>.
        /// </summary>
        private class CharrArrayHtmlContent : IHtmlContent
        {
            public CharrArrayHtmlContent(char[] value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                Value = value;
            }

            public char[] Value { get; }

            /// <inheritdoc />
            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                writer.Write(Value);
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return new string(Value);
            }
        }

        /// <summary>
        /// An <see cref="IHtmlContent"/> implementation that wraps an HTML encoded <see langword="char[]"/>.
        /// </summary>
        private class CharrArrayFragmentHtmlContent : IHtmlContent
        {
            public CharrArrayFragmentHtmlContent(char[] value, int index, int length)
            {
                Value = value;
                Index = index;
                Length = length;
            }

            public char[] Value { get; }
            public int Index { get; }
            public int Length { get; }

            /// <inheritdoc />
            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                writer.Write(Value, Index, Length);
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return new string(Value, Index, Length);
            }
        }
    }
}
