using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement
{
    public struct HtmlContentFragment
    {
        public string StringValue;
        public char[] CharArrayValue;
        private int _index;
        private int _length;
        public char CharValue;

        public HtmlContentFragment(string stringValue)
        {
            StringValue = stringValue;
            CharArrayValue = null;
            CharValue = Char.MinValue;
            _index = 0;
            _length = 0;
        }

        public HtmlContentFragment(char[] charArrayValue, int index, int length)
        {
            StringValue = null;
            CharArrayValue = charArrayValue;
            CharValue = Char.MinValue;
            _index = index;
            _length = length;
        }

        public HtmlContentFragment(char charValue)
        {
            StringValue = null;
            CharArrayValue = null;
            CharValue = charValue;
            _index = 0;
            _length = 0;
        }

        public void WriteTo(TextWriter writer)
        {
            if (StringValue != null)
            {
                writer.Write(StringValue);
            }
            else if (CharArrayValue != null)
            {
                writer.Write(CharArrayValue, _index, _length);
            }
            else
            {
                writer.Write(CharValue);
            }
        }

        public override string ToString()
        {
            if (StringValue != null)
            {
                return StringValue;
            }
            else if (CharArrayValue != null)
            {
                return new string(CharArrayValue, _index, _length);
            }
            else
            {
                return CharValue.ToString();
            }
        }
    }

    public class HtmlContentWriter : TextWriter, IHtmlContent
    {
        private List<HtmlContentFragment> _fragments = new List<HtmlContentFragment>();

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

        public override string ToString()
        {
            return String.Concat(_fragments.ToString());
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
}
