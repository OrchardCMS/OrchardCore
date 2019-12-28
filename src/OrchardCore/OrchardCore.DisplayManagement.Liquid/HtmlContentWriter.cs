using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class HtmlContentWriter : TextWriter, IHtmlContent
    {
        private static readonly string[] _internedChars;
        private const int _internedCharsLength = 256;

        private readonly List<object> _fragments = new List<object>();

        static HtmlContentWriter()
        {
            // Memorize all ASCII chars to prevent allocations
            _internedChars = new string[_internedCharsLength];

            for (var i = 0; i < _internedCharsLength; i++)
            {
                _internedChars[i] = ((char)i).ToString();
            }
        }

        public override Encoding Encoding => Encoding.UTF8;

        // Invoked when used as TextWriter to intercept what is supposed to be written
        public override void Write(string value)
        {
            _fragments.Add(value);
        }

        public override void Write(char value)
        {
            if (value < _internedCharsLength)
            {
                _fragments.Add(_internedChars[value]);
            }
            else
            {
                _fragments.Add(value.ToString());
            }
        }

        public override void Write(char[] buffer)
        {
            _fragments.Add(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (index == 0 && buffer.Length == count)
            {
                _fragments.Add(buffer);
            }
            else
            {
                _fragments.Add(new CharArrayFragment(buffer, index, count));
            }
        }

        // Invoked by IHtmlContent when rendered on the final output
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            foreach (var fragment in _fragments)
            {
                if (fragment is char[] charArray)
                {
                    writer.Write(charArray, 0, charArray.Length);
                }
                else if (fragment is string stringValue)
                {
                    writer.Write(stringValue);
                }
                else if (fragment is CharArrayFragment charArrayFragment)
                {
                    writer.Write(charArrayFragment.CharArray, charArrayFragment.Index, charArrayFragment.Length);
                }
                else
                {
                    throw new NotSupportedException("Unexpected fragment type: " + fragment.GetType());
                }
            }
        }

        private class CharArrayFragment
        {
            public char[] CharArray;
            public int Index;
            public int Length;

            public CharArrayFragment(char[] charArrayValue, int index, int length)
            {
                CharArray = charArrayValue;
                Index = index;
                Length = length;
            }
        }
    }
}
