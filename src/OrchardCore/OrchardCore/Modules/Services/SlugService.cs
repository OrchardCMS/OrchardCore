using System;
using System.Globalization;
using System.Text;
using Cysharp.Text;

namespace OrchardCore.Modules.Services
{
    public class SlugService : ISlugService
    {
        private const char Hyphen = '-';
        private const int MaxLength = 1000;

        public string Slugify(string text) => Slugify(text, Hyphen);

        public string Slugify(string text, char separator)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            var appendSeparator = false;
            var normalizedText = text.Normalize(NormalizationForm.FormKD);

            using var slug = ZString.CreateStringBuilder();

            for (var i = 0; i < normalizedText.Length; i++)
            {
                var currentChar = Char.ToLowerInvariant(normalizedText[i]);

                if (CharUnicodeInfo.GetUnicodeCategory(currentChar) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (Char.IsLetterOrDigit(currentChar))
                {
                    slug.Append(currentChar);

                    appendSeparator = true;
                }
                else if (currentChar == separator)
                {
                    if (appendSeparator && i != normalizedText.Length - 1)
                    {
                        slug.Append(currentChar);
                        appendSeparator = false;
                    }
                }
                else if (currentChar == '_' || currentChar == '~')
                {
                    slug.Append(currentChar);
                }
                else
                {
                    if (appendSeparator)
                    {
                        slug.Append(separator);

                        appendSeparator = false;
                    }
                }
            }

            return new string(slug.AsSpan()[..Math.Min(slug.Length, MaxLength)]).Normalize(NormalizationForm.FormC);
        }
    }
}
