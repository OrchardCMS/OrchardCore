using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OrchardCore.Localization.PortableObject
{
    public class PoParser
    {
        private static readonly Dictionary<char, char> _escapeTranslations = new Dictionary<char, char> {
            { 'n', '\n' },
            { 'r', '\r' },
            { 't', '\t' }
        };

        public IEnumerable<CultureDictionaryRecord> Parse(TextReader reader)
        {
            var entryBuilder = new DictionaryRecordBuilder();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                (var context, var content) = ParseLine(line);

                if (context == PoContext.Other)
                {
                    continue;
                }

                // msgid or msgctxt are first lines of the entry. If builder contains valid entry return it and start building a new one.
                if ((context == PoContext.MessageId || context == PoContext.MessageContext) && entryBuilder.ShouldFlushRecord)
                {
                    yield return entryBuilder.BuildRecordAndReset();
                }

                entryBuilder.Set(context, content);
            }

            if (entryBuilder.ShouldFlushRecord)
            {
                yield return entryBuilder.BuildRecordAndReset();
            }
        }

        private string Unescape(string str)
        {
            StringBuilder sb = null;
            var escaped = false;
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (escaped)
                {
                    if (sb == null)
                    {
                        sb = new StringBuilder(str.Length);
                        if (i > 1)
                        {
                            sb.Append(str.Substring(0, i - 1));
                        }
                    }
                    char unescaped;
                    if (_escapeTranslations.TryGetValue(c, out unescaped))
                    {
                        sb.Append(unescaped);
                    }
                    else
                    {
                        // General rule: \x ==> x
                        sb.Append(c);
                    }
                    escaped = false;
                }
                else
                {
                    if (c == '\\')
                    {
                        escaped = true;
                    }
                    else if (sb != null)
                    {
                        sb.Append(c);
                    }
                }
            }
            return sb == null ? str : sb.ToString();
        }

        private string TrimQuote(string str)
        {
            if (str.StartsWith("\"") && str.EndsWith("\""))
            {
                if (str.Length == 1)
                {
                    return "";
                }

                return str.Substring(1, str.Length - 2);
            }

            return str;
        }

        private (PoContext context, string content) ParseLine(string line)
        {
            if (line.StartsWith("\""))
            {
                return (PoContext.Text, Unescape(TrimQuote(line.Trim())));
            }

            var keyAndValue = line.Split(null, 2);
            if (keyAndValue.Length != 2)
            {
                return (PoContext.Other, string.Empty);
            }

            var content = Unescape(TrimQuote(keyAndValue[1].Trim()));
            switch (keyAndValue[0])
            {
                case "msgctxt": return (PoContext.MessageContext, content);
                case "msgid": return (PoContext.MessageId, content);
                case var key when key.StartsWith("msgstr"): return (PoContext.Translation, content);
                default: return (PoContext.Other, content);
            }
        }

        private class DictionaryRecordBuilder
        {
            private List<string> _values;
            private IEnumerable<string> _validValues => _values.Where(value => !string.IsNullOrEmpty(value));
            private PoContext _context;

            public string MessageId { get; private set; }
            public string MessageContext { get; private set; }

            public IEnumerable<string> Values => _values;

            public bool IsValid => !string.IsNullOrEmpty(MessageId) && _validValues.Any();
            public bool ShouldFlushRecord => IsValid && _context == PoContext.Translation;

            public DictionaryRecordBuilder()
            {
                _values = new List<string>();
            }

            public void Set(PoContext context, string text)
            {
                switch (context)
                {
                    case PoContext.MessageId:
                        {
                            // If the MessageId has been set to an empty string and now gets set again 
                            // before flushing the values should be reset.
                            if (string.IsNullOrEmpty(MessageId))
                            {
                                _values.Clear();
                            }
                            
                            MessageId = text; 
                            break;
                        }
                    case PoContext.MessageContext: MessageContext = text; break;
                    case PoContext.Translation: _values.Add(text); break;
                    case PoContext.Text: AppendText(text); return; // we don't want to set context to Text
                }

                _context = context;
            }

            private void AppendText(string text)
            {
                switch (_context)
                {
                    case PoContext.MessageId: MessageId += text; break;
                    case PoContext.MessageContext: MessageContext += text; break;
                    case PoContext.Translation:
                        if (_values.Count > 0)
                        {
                            _values[_values.Count - 1] += text;
                        }
                        break;
                }
            }

            public CultureDictionaryRecord BuildRecordAndReset()
            {
                if (!IsValid)
                {
                    return null;
                }

                var result = new CultureDictionaryRecord(MessageId, MessageContext, _validValues.ToArray());

                MessageId = null;
                MessageContext = null;
                _values.Clear();

                return result;
            }
        }

        private enum PoContext
        {
            MessageId,
            MessageContext,
            Translation,
            Text,
            Other
        }
    }
}
