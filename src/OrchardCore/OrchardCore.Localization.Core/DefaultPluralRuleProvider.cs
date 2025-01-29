using System.Globalization;

namespace OrchardCore.Localization;

/// <summary>
/// Provides pluralization rules based on the Unicode Common Locale Data Repository.
/// c.f. http://www.unicode.org/cldr/charts/latest/supplemental/language_plural_rules.html
/// c.f. https://github.com/unicode-org/cldr/blob/master/common/supplemental/plurals.xml.
/// </summary>
public class DefaultPluralRuleProvider : IPluralRuleProvider
{
    private static readonly Dictionary<string, PluralizationRuleDelegate> _rules;

    static DefaultPluralRuleProvider()
    {
        _rules = [];

        AddRule(["ay", "bo", "cgg", "dz", "fa", "id", "ja", "jbo", "ka", "kk", "km", "ko", "ky", "lo", "ms", "my", "sah", "su", "th", "tt", "ug", "vi", "wo", "zh"], n => 0);
        AddRule(["ach", "ak", "am", "arn", "br", "fil", "fr", "gun", "ln", "mfe", "mg", "mi", "oc", "pt-BR", "tg", "ti", "tr", "uz", "wa"], n => (n > 1 ? 1 : 0));
        AddRule(["af", "an", "anp", "as", "ast", "az", "bg", "bn", "brx", "ca", "da", "de", "doi", "el", "en", "eo", "es", "es-AR", "et", "eu", "ff", "fi", "fo", "fur", "fy", "gl", "gu", "ha", "he", "hi", "hne", "hu", "hy", "ia", "it", "kl", "kn", "ku", "lb", "mai", "ml", "mn", "mni", "mr", "nah", "nap", "nb", "ne", "nl", "nn", "no", "nso", "or", "pa", "pap", "pms", "ps", "pt", "rm", "rw", "sat", "sco", "sd", "se", "si", "so", "son", "sq", "sv", "sw", "ta", "te", "tk", "ur", "yo"], n => (n != 1 ? 1 : 0));
        AddRule(["is"], n => (n % 10 != 1 || n % 100 == 11 ? 1 : 0));
        AddRule(["jv"], n => (n != 0 ? 1 : 0));
        AddRule(["mk"], n => (n == 1 || n % 10 == 1 ? 0 : 1));
        AddRule(["be", "bs", "hr", "lt"], n => (n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2));
        AddRule(["cs"], n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2));
        AddRule(["csb", "pl"], n => ((n == 1) ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2));
        AddRule(["lv"], n => (n % 10 == 1 && n % 100 != 11 ? 0 : n != 0 ? 1 : 2));
        AddRule(["mnk"], n => (n == 0 ? 0 : n == 1 ? 1 : 2));
        AddRule(["ro"], n => (n == 1 ? 0 : (n == 0 || (n % 100 > 0 && n % 100 < 20)) ? 1 : 2));
        AddRule(["cy"], n => ((n == 1) ? 0 : (n == 2) ? 1 : (n != 8 && n != 11) ? 2 : 3));
        AddRule(["gd"], n => ((n == 1 || n == 11) ? 0 : (n == 2 || n == 12) ? 1 : (n > 2 && n < 20) ? 2 : 3));
        AddRule(["kw"], n => ((n == 1) ? 0 : (n == 2) ? 1 : (n == 3) ? 2 : 3));
        AddRule(["mt"], n => (n == 1 ? 0 : n == 0 || (n % 100 > 1 && n % 100 < 11) ? 1 : (n % 100 > 10 && n % 100 < 20) ? 2 : 3));
        AddRule(["sl"], n => (n % 100 == 1 ? 1 : n % 100 == 2 ? 2 : n % 100 == 3 || n % 100 == 4 ? 3 : 0));
        AddRule(["ru", "sr", "uk"], n => (n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2));
        AddRule(["sk"], n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2));
        AddRule(["ga"], n => (n == 1 ? 0 : n == 2 ? 1 : (n > 2 && n < 7) ? 2 : (n > 6 && n < 11) ? 3 : 4));
        AddRule(["ar"], n => (n == 0 ? 0 : n == 1 ? 1 : n == 2 ? 2 : n % 100 >= 3 && n % 100 <= 10 ? 3 : n % 100 >= 11 ? 4 : 5));
    }

    /// <inheritdocs />
    public int Order => 0;

    /// <inheritdocs />
    public bool TryGetRule(CultureInfo culture, out PluralizationRuleDelegate rule)
    {
        var cultureForPlural = GetBaseCulture(culture);

        return _rules.TryGetValue(cultureForPlural.Name, out rule);
    }

    private static void AddRule(string[] cultures, PluralizationRuleDelegate rule)
    {
        foreach (var culture in cultures)
        {
            _rules.Add(culture, rule);
        }
    }

    /// <example>zh-Hans-CN -> zh-Hans -> zh.</example>
    private static CultureInfo GetBaseCulture(CultureInfo culture)
    {
        var returnCulture = culture;

        while (returnCulture.Parent.Name != "") // Stop at Invariant culture
        {
            returnCulture = returnCulture.Parent;
        }

        return returnCulture;
    }
}
