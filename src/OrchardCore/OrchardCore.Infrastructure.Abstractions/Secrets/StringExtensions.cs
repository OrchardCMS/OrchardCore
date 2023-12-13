using System;
using System.Globalization;
using System.Text;

namespace OrchardCore.Secrets
{
    public static class StringExtensions
    {
        /// <summary>
        /// Generates a safe secret name allowing the '.' delimiter.
        /// </summary>
        public static string ToSafeSecretName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            name = RemoveDiacritics(name);
            name = name.Strip(c => !c.IsLetter() && !char.IsDigit(c) && c != '.');

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
    }
}
