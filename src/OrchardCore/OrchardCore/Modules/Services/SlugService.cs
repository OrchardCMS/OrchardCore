using System.Globalization;
using System.Text;
using Cysharp.Text;

namespace OrchardCore.Modules.Services;

public class SlugService : ISlugService
{
    private const char Hyphen = '-';
    private const int MaxLength = 1000;

    public string Slugify(string text, char separator)
    {
        throw new NotImplementedException();
    }

    public string Slugify(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var appendHyphen = false;
        var normalizedText = text.Normalize(NormalizationForm.FormKD);

        using var slug = ZString.CreateStringBuilder();

        for (var i = 0; i < normalizedText.Length; i++)
        {
            var currentChar = char.ToLowerInvariant(normalizedText[i]);

            if (CharUnicodeInfo.GetUnicodeCategory(currentChar) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(currentChar))
            {
                slug.Append(currentChar);

                appendHyphen = true;
            }
            else if (currentChar is Hyphen)
            {
                if (appendHyphen && i != normalizedText.Length - 1)
                {
                    slug.Append(currentChar);
                    appendHyphen = false;
                }
            }
            else if (currentChar == '_' || currentChar == '~')
            {
                slug.Append(currentChar);
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

        var length = Math.Min(slug.Length - GetTrailingHyphenCount(slug.AsSpan()), MaxLength);

        return new string(slug.AsSpan()[..length]).Normalize(NormalizationForm.FormC);
    }

    private static int GetTrailingHyphenCount(ReadOnlySpan<char> input)
    {
        var hyphenCount = 0;
        for (var i = input.Length - 1; i >= 0; i--)
        {
            if (input[i] != Hyphen)
            {
                break;
            }

            ++hyphenCount;
        }

        return hyphenCount;
    }
}
