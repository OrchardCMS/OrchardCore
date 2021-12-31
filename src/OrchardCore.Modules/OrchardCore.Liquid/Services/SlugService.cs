using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OrchardCore.Liquid.Services
{
    public class SlugService : ISlugService
    {
        private const char Hyphen = '-';
        private const int MaxLength = 1000;

        private static readonly char[] _allowedSymbols = new char[] { Hyphen, '_', '~' };

        public string Slugify(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            var slug = new StringBuilder();
            var appendHyphen = false;
            var normalizedText = text.ToLowerInvariant().Normalize(NormalizationForm.FormKD);

            for (var i = 0; i < normalizedText.Length; i++)
            {
                var currentChar = normalizedText[i];

                if (CharUnicodeInfo.GetUnicodeCategory(currentChar) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (Char.IsLetterOrDigit(currentChar))
                {
                    slug.Append(currentChar);

                    appendHyphen = true;
                }
                else if(_allowedSymbols.Contains(currentChar))
                {
                    if (currentChar == Hyphen)
                    {
                        if (appendHyphen && i != normalizedText.Length - 1)
                        {
                            slug.Append(currentChar);

                            appendHyphen = false;
                        }
                    }
                    else
                    {
                        slug.Append(currentChar);
                    }
                }
                else
                {
                    if (appendHyphen)
                    {
                        slug.Append(Hyphen);

                        appendHyphen = false;
                    }
                }
            }
            return slug
                .ToString()[..Math.Min(slug.Length, MaxLength)]
                .Normalize(NormalizationForm.FormC);
        }
    }
}
