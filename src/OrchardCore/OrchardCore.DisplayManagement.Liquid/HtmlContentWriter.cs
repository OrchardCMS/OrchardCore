using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class HtmlContentWriter : TextWriter, IHtmlContent
    {
        private List<HtmlContentFragment> _fragments = new List<HtmlContentFragment>();
        private List<char[]> _rented = new List<char[]>();

        public override Encoding Encoding => Encoding.UTF8;

        // Invoked when used as TextWriter to intercept what is supposed to be written
        public override void Write(string value)
        {
            _fragments.Add(new HtmlContentFragment(value));
        }

        public override void Write(char value)
        {
            _fragments.Add(new HtmlContentFragment(value));
        }

        public override void Write(char[] buffer, int index, int count)
        {
            _fragments.Add(new HtmlContentFragment(buffer, index, count));
        }

        public override void Write(ReadOnlySpan<char> buffer)
        {
            char[] array = ArrayPool<char>.Shared.Rent(buffer.Length);
            _rented.Add(array);

            buffer.CopyTo(new Span<char>(array));

            _fragments.Add(new HtmlContentFragment(array, 0, buffer.Length));
        }

        ~HtmlContentWriter()
        {
            foreach (var array in _rented)
            {
                ArrayPool<char>.Shared.Return(array);
            }

            _rented.Clear();
        }

        public override string ToString()
        {
            return String.Concat(_fragments.Select(x => x.ToString()));
        }

        // Invoked by IHtmlContent when rendered on the final output
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            foreach (var fragment in _fragments)
            {
                fragment.WriteTo(writer);
            }
        }
    }

    public struct HtmlContentFragment
    {
        private string _stringValue;
        private char[] _charArrayValue;
        private char _charValue;
        private int _index;
        private int _length;

        public HtmlContentFragment(string stringValue)
        {
            _stringValue = stringValue;
            _charArrayValue = null;
            _charValue = Char.MinValue;
            _index = 0;
            _length = 0;
        }

        public HtmlContentFragment(char[] charArrayValue, int index, int length)
        {
            _stringValue = null;
            _charArrayValue = charArrayValue;
            _charValue = Char.MinValue;
            _index = index;
            _length = length;
        }

        public HtmlContentFragment(char charValue)
        {
            _stringValue = null;
            _charArrayValue = null;
            _charValue = charValue;
            _index = 0;
            _length = 0;
        }

        public void WriteTo(TextWriter writer)
        {
            if (_stringValue != null)
            {
                writer.Write(_stringValue);
            }
            else if (_charArrayValue != null)
            {
                writer.Write(_charArrayValue, _index, _length);
            }
            else
            {
                writer.Write(_charValue);
            }
        }

        public override string ToString()
        {
            if (_stringValue != null)
            {
                return _stringValue;
            }
            else if (_charArrayValue != null)
            {
                return new string(_charArrayValue, _index, _length);
            }
            else
            {
                return _charValue.ToString();
            }
        }
    }
}
