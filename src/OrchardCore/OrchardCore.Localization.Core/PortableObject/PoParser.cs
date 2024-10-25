using System.Collections.Frozen;
using Cysharp.Text;

namespace OrchardCore.Localization.PortableObject;

/// <summary>
/// Represents a parser for portable objects.
/// </summary>
public class PoParser
{
    private static readonly FrozenDictionary<char, char> _escapeTranslations;

    static PoParser()
    {
        _escapeTranslations = new Dictionary<char, char>()
        {
            { 'n', '\n' },
            { 'r', '\r' },
            { 't', '\t' }
        }.ToFrozenDictionary();
    }

    /// <summary>
    /// Parses a .po file.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/>.</param>
    /// <returns>A list of culture records.</returns>
#pragma warning disable CA1822 // Mark members as static
    public IEnumerable<CultureDictionaryRecord> Parse(TextReader reader)
#pragma warning restore CA1822 // Mark members as static
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

    private static string Unescape(string str)
    {
        if (!str.Contains('\\'))
        {
            return str;
        }

        var escaped = false;
        using var builder = ZString.CreateStringBuilder();

        for (var i = 0; i < str.Length; i++)
        {
            var c = str[i];
            if (escaped)
            {
                char unescaped;
                if (_escapeTranslations.TryGetValue(c, out unescaped))
                {
                    builder.Append(unescaped);
                }
                else
                {
                    // General rule: \x ==> x
                    builder.Append(c);
                }

                escaped = false;
            }
            else
            {
                if (c == '\\')
                {
                    escaped = true;
                }
                else
                {
                    builder.Append(c);
                }
            }
        }

        return builder.ToString();
    }

    private static string TrimQuote(string str)
    {
        if (str.StartsWith('\"') && str.EndsWith('\"'))
        {
            if (str.Length == 1)
            {
                return "";
            }

            return str[1..^1];
        }

        return str;
    }

    private static (PoContext context, string content) ParseLine(string line)
    {
        if (line.StartsWith('\"'))
        {
            return (PoContext.Text, Unescape(TrimQuote(line.Trim())));
        }

        var keyAndValue = line.Split(null, 2);
        if (keyAndValue.Length != 2)
        {
            return (PoContext.Other, string.Empty);
        }

        var content = Unescape(TrimQuote(keyAndValue[1].Trim()));
        return keyAndValue[0] switch
        {
            "msgctxt" => (PoContext.MessageContext, content),
            "msgid" => (PoContext.MessageId, content),
            "msgid_plural" => (PoContext.MessageIdPlural, content),
            var key when key.StartsWith("msgstr", StringComparison.Ordinal) => (PoContext.Translation, content),
            _ => (PoContext.Other, content),
        };
    }

    private sealed class DictionaryRecordBuilder
    {
        private readonly List<string> _values;
        private IEnumerable<string> _validValues => _values.Where(value => !string.IsNullOrEmpty(value));
        private PoContext _context;

        public string MessageId { get; private set; }
        public string MessageContext { get; private set; }

        public IEnumerable<string> Values => _values;

        public bool IsValid => !string.IsNullOrEmpty(MessageId) && _validValues.Any();
        public bool ShouldFlushRecord => IsValid && _context == PoContext.Translation;

        public DictionaryRecordBuilder()
        {
            _values = [];
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
                case PoContext.Text: AppendText(text); return; // We don't want to set context to Text.
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
                        _values[^1] += text;
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
        MessageIdPlural,
        MessageContext,
        Translation,
        Text,
        Other
    }
}
