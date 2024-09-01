using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Text;
using Microsoft.Extensions.Localization;

namespace OrchardCore.ContentManagement.Utilities;

public static class StringExtensions
{
    private static readonly char[] _validSegmentChars = "/?#[]@\"^{}|`<>\t\r\n\f ".ToCharArray();

    private static readonly HashSet<string> _reservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(ContentItem.Id),
        nameof(ContentItem.ContentItemId),
        nameof(ContentItem.ContentItemVersionId),
        nameof(ContentItem.ContentType),
        nameof(ContentItem.Published),
        nameof(ContentItem.Latest),
        nameof(ContentItem.ModifiedUtc),
        nameof(ContentItem.PublishedUtc),
        nameof(ContentItem.CreatedUtc),
        nameof(ContentItem.Owner),
        nameof(ContentItem.Author),
        nameof(ContentItem.DisplayText),
    };

    public static string CamelFriendly(this string camel)
    {
        // optimize common cases
        if (string.IsNullOrWhiteSpace(camel))
        {
            return "";
        }

        using var sb = ZString.CreateStringBuilder();
        for (var i = 0; i < camel.Length; ++i)
        {
            var c = camel[i];
            if (i != 0 && char.IsUpper(c))
            {
                sb.Append(' ');
            }
            sb.Append(c);
        }

        return sb.ToString();
    }

    public static string Ellipsize(this string text, int characterCount) => text.Ellipsize(characterCount, "\u00A0\u2026");

    public static string Ellipsize(this string text, int characterCount, string ellipsis, bool wordBoundary = false)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (characterCount < 0 || text.Length <= characterCount)
        {
            return text;
        }

        // search beginning of word
        var backup = characterCount;
        while (characterCount > 0 && char.IsLetter(text[characterCount - 1]))
        {
            characterCount--;
        }

        // search previous word
        while (characterCount > 0 && char.IsWhiteSpace(text[characterCount - 1]))
        {
            characterCount--;
        }

        // if it was the last word, recover it, unless boundary is requested
        if (characterCount == 0 && !wordBoundary)
        {
            characterCount = backup;
        }

        var trimmed = text[..characterCount];

        return trimmed + ellipsis;
    }

    public static LocalizedString OrDefault(this string text, LocalizedString defaultValue)
        => string.IsNullOrEmpty(text)
            ? defaultValue
            : new LocalizedString(null, text);

    public static string RemoveTags(this string html, bool htmlDecode = false)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        var result = new char[html.Length];

        var cursor = 0;
        var inside = false;
        for (var i = 0; i < html.Length; i++)
        {
            var current = html[i];

            switch (current)
            {
                case '<':
                    inside = true;
                    continue;
                case '>':
                    inside = false;
                    continue;
            }

            if (!inside)
            {
                result[cursor++] = current;
            }
        }

        var stringResult = new string(result, 0, cursor);

        if (htmlDecode)
        {
            stringResult = WebUtility.HtmlDecode(stringResult);
        }

        return stringResult;
    }

    // not accounting for only \r (e.g. Apple OS 9 carriage return only new lines)
    public static string ReplaceNewLinesWith(this string text, string replacement)
        => string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : text.Replace("\r\n", "\r\r")
                .Replace("\n", string.Format(replacement, "\r\n"))
                .Replace("\r\r", string.Format(replacement, "\r\n"));

    public static bool IsValidUrlSegment(this string segment)
    {
        // valid isegment from rfc3987 - http://tools.ietf.org/html/rfc3987#page-8
        // the relevant bits:
        // isegment    = *ipchar
        // ipchar      = iunreserved / pct-encoded / sub-delims / ":" / "@"
        // iunreserved = ALPHA / DIGIT / "-" / "." / "_" / "~" / ucschar
        // pct-encoded = "%" HEXDIG HEXDIG
        // sub-delims  = "!" / "$" / "&" / "'" / "(" / ")" / "*" / "+" / "," / ";" / "="
        // ucschar     = %xA0-D7FF / %xF900-FDCF / %xFDF0-FFEF / %x10000-1FFFD / %x20000-2FFFD / %x30000-3FFFD / %x40000-4FFFD / %x50000-5FFFD / %x60000-6FFFD / %x70000-7FFFD / %x80000-8FFFD / %x90000-9FFFD / %xA0000-AFFFD / %xB0000-BFFFD / %xC0000-CFFFD / %xD0000-DFFFD / %xE1000-EFFFD
        //
        // rough blacklist regex == m/^[^/?#[]@"^{}|\s`<>]+$/ (leaving off % to keep the regex simple)

        return !segment.Any(_validSegmentChars);
    }

    /// <summary>
    /// Generates a valid technical name.
    /// </summary>
    /// <remarks>
    /// Uses a white list set of chars.
    /// </remarks>
    public static string ToSafeName(this string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        name = RemoveDiacritics(name);

        name = name.Strip(c => !char.IsLetter(c) && !char.IsDigit(c)).Trim();

        // don't allow non A-Z chars as first letter, as they are not allowed in prefixes
        while (name.Length > 0 && !char.IsLetter(name[0]))
        {
            name = name[1..];
        }

        if (name.Length > 128)
        {
            name = name[..128];
        }

        return name;
    }

    [Obsolete("Use Char.IsLetter() instead.")]
    public static bool IsLetter(this char c)
    {
        return ('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z');
    }

    [Obsolete("Use Char.IsWhiteSpace() instead.")]
    public static bool IsSpace(this char c)
    {
        return c == '\r' || c == '\n' || c == '\t' || c == '\f' || c == ' ';
    }

    public static bool IsReservedContentName(this string name) => _reservedNames.Contains(name);

    public static string RemoveDiacritics(this string name)
    {
        var stFormD = name.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var t in stFormD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(t);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(t);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string Strip(this string source, params char[] stripped)
    {
        if (stripped == null || stripped.Length == 0 || string.IsNullOrEmpty(source))
        {
            return source;
        }

        var result = new char[source.Length];

        var cursor = 0;
        for (var i = 0; i < source.Length; i++)
        {
            var current = source[i];
            if (Array.IndexOf(stripped, current) < 0)
            {
                result[cursor++] = current;
            }
        }

        return new string(result, 0, cursor);
    }

    public static string Strip(this string source, Func<char, bool> predicate)
    {
        var result = new char[source.Length];

        var cursor = 0;
        for (var i = 0; i < source.Length; i++)
        {
            var current = source[i];
            if (!predicate(current))
            {
                result[cursor++] = current;
            }
        }

        return new string(result, 0, cursor);
    }

    public static bool Any(this string source, params char[] chars)
    {
        if (string.IsNullOrEmpty(source) || chars == null || chars.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < source.Length; i++)
        {
            var current = source[i];
            if (Array.IndexOf(chars, current) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    public static bool All(this string source, params char[] chars)
    {
        if (string.IsNullOrEmpty(source) || chars == null || chars.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < source.Length; i++)
        {
            var current = source[i];
            if (Array.IndexOf(chars, current) < 0)
            {
                return false;
            }
        }

        return true;
    }

    public static string Translate(this string subject, char[] from, char[] to)
    {
        if (string.IsNullOrEmpty(subject))
        {
            return subject;
        }

        ArgumentNullException.ThrowIfNull(from);

        ArgumentNullException.ThrowIfNull(to);

        if (from.Length != to.Length)
        {
            throw new ArgumentNullException(nameof(from), "Parameters must have the same length");
        }

        var map = new Dictionary<char, char>(from.Length);
        for (var i = 0; i < from.Length; i++)
        {
            map[from[i]] = to[i];
        }

        var result = new char[subject.Length];

        for (var i = 0; i < subject.Length; i++)
        {
            var current = subject[i];
            if (map.TryGetValue(current, out var value))
            {
                result[i] = value;
            }
            else
            {
                result[i] = current;
            }
        }

        return new string(result);
    }

    public static string ReplaceAll(this string original, IDictionary<string, string> replacements)
    {
        var pattern = $"{string.Join("|", replacements.Keys)}";

        return Regex.Replace(original, pattern, match => replacements[match.Value]);
    }

#if NET8_0
    [Obsolete("Don't use 'TrimEnd' as this has a different behavior in .NET 9.0. Use 'OrchardCore.ContentManagement.Utilities.TrimEndString' instead.")]
    public static string TrimEnd(this string value, string trim = "")
    {
        if (value == null)
        {
            return null;
        }

        return value.EndsWith(trim, StringComparison.Ordinal)
            ? value[..^trim.Length]
            : value;
    }
#endif

    public static string TrimEndString(this string value, string suffix)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return value.EndsWith(suffix, StringComparison.Ordinal)
            ? value[..^suffix.Length]
            : value;
    }

    public static string ReplaceLastOccurrence(this string source, string searchedValue, string replacedValue)
    {
        if (searchedValue is null || replacedValue is null)
        {
            return source;
        }

        var lastIndex = source.LastIndexOf(searchedValue, StringComparison.Ordinal);
        if (lastIndex == -1)
        {
            return source;
        }

        return source.Remove(lastIndex, searchedValue.Length).Insert(lastIndex, replacedValue);
    }
}
