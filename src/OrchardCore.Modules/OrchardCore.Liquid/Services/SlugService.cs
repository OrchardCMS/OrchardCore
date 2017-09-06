using System;
using System.Globalization;
using System.Text;

namespace OrchardCore.Liquid.Services
{
    public class SlugService : ISlugService
    {
        public string Slugify(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var sb = new StringBuilder();

            var stFormKD = text.Trim().ToLower().Normalize(NormalizationForm.FormKD);
            foreach (var t in stFormKD)
            {
                // Allowed symbols
                if (t == '-' || t == '_' || t == '~')
                {
                    sb.Append(t);
                    continue;
                }

                var uc = CharUnicodeInfo.GetUnicodeCategory(t);
                switch (uc)
                {
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        // Keep letters and digits
                        sb.Append(t);
                        break;
                    case UnicodeCategory.NonSpacingMark:
                        // Remove diacritics
                        break;
                    default:
                        // Replace all other chars with dash
                        sb.Append('-');
                        break;
                }
            }

            var slug = sb.ToString().Normalize(NormalizationForm.FormC);

            // Simplifies dash groups
            for (var i = 0; i < slug.Length - 1; i++)
            {
                if (slug[i] == '-')
                {
                    var j = 0;
                    while (i + j + 1 < slug.Length && slug[i + j + 1] == '-')
                    {
                        j++;
                    }
                    if (j > 0)
                    {
                        slug = slug.Remove(i + 1, j);
                    }
                }
            }

            if (slug.Length > 1000)
            {
                slug = slug.Substring(0, 1000);
            }

            slug = slug.Trim('-', '_', '.');

            return slug;
        }
    }
}
