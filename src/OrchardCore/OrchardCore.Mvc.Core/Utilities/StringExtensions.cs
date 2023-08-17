using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Text;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Mvc.Utilities
{
    public static class StringExtensions
    {
        public static string CamelFriendly(this string camel)
        {
            // Optimize common cases.
            if (String.IsNullOrWhiteSpace(camel))
            {
                return "";
            }

            using var sb = ZString.CreateStringBuilder();
            for (var i = 0; i < camel.Length; ++i)
            {
                var c = camel[i];
                if (i != 0 && Char.IsUpper(c))
                {
                    sb.Append(' ');
                }
                sb.Append(c);
            }

            return sb.ToString();
        }

        public static string Ellipsize(this string text, int characterCount)
        {
            return text.Ellipsize(characterCount, "\u00A0\u2026");
        }

        public static string Ellipsize(this string text, int characterCount, string ellipsis, bool wordBoundary = false)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            if (characterCount < 0 || text.Length <= characterCount)
                return text;

            // Search beginning of word.
            var backup = characterCount;
            while (characterCount > 0 && text[characterCount - 1].IsLetter())
            {
                characterCount--;
            }

            // Search previous word.
            while (characterCount > 0 && text[characterCount - 1].IsSpace())
            {
                characterCount--;
            }

            // If it was the last word, recover it, unless boundary is requested.
            if (characterCount == 0 && !wordBoundary)
            {
                characterCount = backup;
            }

            var trimmed = text[..characterCount];
            return trimmed + ellipsis;
        }

        public static string HtmlClassify(this string text)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            var friendlier = text.CamelFriendly();

            var result = new char[friendlier.Length];

            var cursor = 0;
            var previousIsNotLetter = false;
            for (var i = 0; i < friendlier.Length; i++)
            {
                var current = friendlier[i];
                if (IsLetter(current) || (Char.IsDigit(current) && cursor > 0))
                {
                    if (previousIsNotLetter && i != 0 && cursor > 0)
                    {
                        result[cursor++] = '-';
                    }

                    result[cursor++] = Char.ToLowerInvariant(current);
                    previousIsNotLetter = false;
                }
                else
                {
                    previousIsNotLetter = true;
                }
            }

            return new string(result, 0, cursor);
        }

        public static LocalizedString OrDefault(this string text, LocalizedString defaultValue)
        {
            return String.IsNullOrEmpty(text)
                ? defaultValue
                : new LocalizedString(null, text);
        }

        public static string RemoveTags(this string html, bool htmlDecode = false)
        {
            if (String.IsNullOrEmpty(html))
            {
                return String.Empty;
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

        // Not accounting for only \r (e.g. Apple OS 9 carriage return only new lines).
        public static string ReplaceNewLinesWith(this string text, string replacement)
        {
            return String.IsNullOrWhiteSpace(text)
                       ? String.Empty
                       : text
                             .Replace("\r\n", "\r\r")
                             .Replace("\n", String.Format(replacement, "\r\n"))
                             .Replace("\r\r", String.Format(replacement, "\r\n"));
        }

        private static readonly char[] _validSegmentChars = "/?#[]@\"^{}|`<>\t\r\n\f ".ToCharArray();
        public static bool IsValidUrlSegment(this string segment)
        {
            // Valid isegment from rfc3987 - http://tools.ietf.org/html/rfc3987#page-8
            // The relevant bits:
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
            if (String.IsNullOrWhiteSpace(name))
            {
                return String.Empty;
            }

            name = RemoveDiacritics(name);
            name = name.Strip(c =>
                !c.IsLetter()
                && !Char.IsDigit(c)
                );

            name = name.Trim();

            // Don't allow non A-Z chars as first letter, as they are not allowed in prefixes.
            while (name.Length > 0 && !IsLetter(name[0]))
            {
                name = name[1..];
            }

            if (name.Length > 128)
            {
                name = name[..128];
            }

            return name;
        }

        /// <summary>
        /// Whether the char is a letter between A and Z or not.
        /// </summary>
        public static bool IsLetter(this char c)
        {
            return ('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z');
        }

        public static bool IsSpace(this char c)
        {
            return (c == '\r' || c == '\n' || c == '\t' || c == '\f' || c == ' ');
        }

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

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        /// <summary>
        /// Transforms the culture of a letter to its equivalent representation in the 0-127 ascii table, such as the letter 'é' is substituted by an 'e'.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ReplaceDiacritics(this string s)
        {
            var stringBuilder = new StringBuilder();
            var normalizedString = s.Normalize(NormalizationForm.FormD);

            for (var i = 0; i <= normalizedString.Length - 1; i++)
            {
                var c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }

        public static string Strip(this string subject, params char[] stripped)
        {
            if (stripped == null || stripped.Length == 0 || String.IsNullOrEmpty(subject))
            {
                return subject;
            }

            var result = new char[subject.Length];

            var cursor = 0;
            for (var i = 0; i < subject.Length; i++)
            {
                var current = subject[i];
                if (Array.IndexOf(stripped, current) < 0)
                {
                    result[cursor++] = current;
                }
            }

            return new string(result, 0, cursor);
        }

        public static string Strip(this string subject, Func<char, bool> predicate)
        {
            var result = new char[subject.Length];

            var cursor = 0;
            for (var i = 0; i < subject.Length; i++)
            {
                var current = subject[i];
                if (!predicate(current))
                {
                    result[cursor++] = current;
                }
            }

            return new string(result, 0, cursor);
        }

        public static bool Any(this string subject, params char[] chars)
        {
            if (String.IsNullOrEmpty(subject) || chars == null || chars.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < subject.Length; i++)
            {
                var current = subject[i];
                if (Array.IndexOf(chars, current) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool All(this string subject, params char[] chars)
        {
            if (String.IsNullOrEmpty(subject))
            {
                return true;
            }

            if (chars == null || chars.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < subject.Length; i++)
            {
                var current = subject[i];
                if (Array.IndexOf(chars, current) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static string Translate(this string subject, char[] from, char[] to)
        {
            if (String.IsNullOrEmpty(subject))
            {
                return subject;
            }

            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

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
                if (map.ContainsKey(current))
                {
                    result[i] = map[current];
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
            var pattern = $"{String.Join("|", replacements.Keys)}";
            return Regex.Replace(original, pattern, match => replacements[match.Value]);
        }

        public static string TrimEnd(this string rough, string trim = "")
        {
            if (rough == null)
            {
                return null;
            }

            return rough.EndsWith(trim, StringComparison.Ordinal)
                       ? rough[..^trim.Length]
                       : rough;
        }

        public static string ReplaceLastOccurrence(this string source, string find, string replace)
        {
            var place = source.LastIndexOf(find, StringComparison.Ordinal);
            return source.Remove(place, find.Length).Insert(place, replace);
        }

        private static ImmutableDictionary<string, string> _underscorePascalCaseIndex = ImmutableDictionary<string, string>.Empty;
        private static ImmutableDictionary<string, string> _dashPascalCaseIndex = ImmutableDictionary<string, string>.Empty;

        /// <summary>
        /// Converts a liquid attribute to pascal case
        /// </summary>
        public static string ToPascalCaseUnderscore(this string attribute)
        {
            if (!_underscorePascalCaseIndex.TryGetValue(attribute, out var result))
            {
                result = ToPascalCase(attribute, '_');
                _underscorePascalCaseIndex = _underscorePascalCaseIndex.SetItem(attribute, result);
            }

            return result;
        }

        /// <summary>
        /// Converts an html attribute to pascal case.
        /// </summary>
        public static string ToPascalCaseDash(this string attribute)
        {
            if (!_dashPascalCaseIndex.TryGetValue(attribute, out var result))
            {
                result = ToPascalCase(attribute, '-');
                _dashPascalCaseIndex = _dashPascalCaseIndex.SetItem(attribute, result);
            }

            return result;
        }

        /// <summary>
        /// Converts a string to pascal case.
        /// </summary>
        public static string ToPascalCase(this string attribute, char upperAfterDelimiter)
        {
            attribute = attribute.Trim();

            var delimitersCount = 0;

            for (var i = 0; i < attribute.Length; i++)
            {
                if (attribute[i] == upperAfterDelimiter)
                {
                    delimitersCount++;
                }
            }

            var result = String.Create(attribute.Length - delimitersCount, new { attribute, upperAfterDelimiter }, (buffer, state) =>
            {
                var nextIsUpper = true;
                var k = 0;

                for (var i = 0; i < state.attribute.Length; i++)
                {
                    var c = state.attribute[i];

                    if (c == state.upperAfterDelimiter)
                    {
                        nextIsUpper = true;
                        continue;
                    }

                    if (nextIsUpper)
                    {
                        buffer[k] = Char.ToUpperInvariant(c);
                    }
                    else
                    {
                        buffer[k] = c;
                    }

                    nextIsUpper = false;

                    k++;
                }
            });

            return result;
        }

        /// <summary>
        /// Tests if a string is valid json.
        /// </summary>
        public static bool IsJson(this string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
