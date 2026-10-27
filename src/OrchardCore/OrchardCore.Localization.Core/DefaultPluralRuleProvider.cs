using System.Globalization;

namespace OrchardCore.Localization;

/// <summary>
/// Provides integer cardinal pluralization rules based on Unicode CLDR 49.
/// See https://www.unicode.org/cldr/charts/49/supplemental/language_plural_rules.html.
/// </summary>
public class DefaultPluralRuleProvider : IPluralRuleProvider
{
    private static readonly Dictionary<string, PluralizationRuleDelegate> s_rules;

    static DefaultPluralRuleProvider()
    {
        s_rules = [];

        // Retain legacy registrations without explicit CLDR data; do not infer new linguistic rules for them.
        AddRule(["ay", "bm", "bo", "dz", "hnj", "id", "ig", "ii", "in", "ja", "jbo", "jv", "jw", "kde", "kea", "km", "ko", "lkt", "lo", "ms", "my", "nqo", "osa", "sah", "ses", "sg", "su", "th", "to", "tpi", "tt", "wo", "yo", "yue", "zh"], n => 0);
        AddRule(["ach", "ak", "am", "arn", "as", "bho", "bn", "csw", "doi", "fa", "ff", "gu", "gun", "guw", "hi", "hy", "kab", "kn", "kok", "kok-Latn", "ln", "mfe", "mg", "mi", "nso", "oc", "pa", "pcm", "si", "tg", "ti", "vi", "wa", "zu"], n => (n > 1 ? 1 : 0));
        AddRule(["af", "an", "anp", "asa", "ast", "az", "bal", "bem", "bez", "bg", "brx", "ce", "cgg", "chr", "ckb", "da", "de", "dv", "ee", "el", "en", "eo", "et", "eu", "fi", "fo", "fur", "fy", "gsw", "ha", "haw", "hne", "hu", "ia", "ie", "io", "jgo", "ji", "jmc", "ka", "kaj", "kcg", "kk", "kkj", "kl", "ks", "ksb", "ku", "ky", "lb", "lg", "lij", "mai", "mas", "mgo", "ml", "mn", "mni", "mr", "nah", "nap", "nb", "nd", "ne", "nl", "nn", "nnh", "no", "nr", "ny", "nyn", "om", "or", "os", "pap", "pms", "ps", "rm", "rof", "rw", "rwk", "saq", "sc", "sco", "sd", "sdh", "seh", "sn", "so", "son", "sq", "ss", "ssy", "st", "sv", "sw", "syr", "ta", "te", "teo", "tig", "tk", "tn", "tr", "ts", "ug", "ur", "uz", "ve", "vo", "vun", "wae", "xh", "xog", "yi"], n => (n != 1 ? 1 : 0));
        AddRule(["tzm"], n => (n is <= 1 or (>= 11 and <= 99) ? 0 : 1));
        AddRule(["fr", "pt", "pt-BR"], n => GetMillionPluralForm(n, includeZero: true));
        AddRule(["ca", "es", "es-AR", "gl", "it", "lld", "pt-PT", "scn", "vec"], n => GetMillionPluralForm(n, includeZero: false));
        AddRule(["is"], n => (n % 10 != 1 || n % 100 == 11 ? 1 : 0));
        AddRule(["mk"], n => (n % 10 == 1 && n % 100 != 11 ? 0 : 1));
        AddRule(["ceb", "fil", "tl"], n => (n % 10 is 4 or 6 or 9 ? 1 : 0));
        AddRule(["be", "bs", "hr", "ru", "sh", "sr", "uk"], n => (n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2));
        AddRule(["lt"], n => (n % 10 == 1 && n % 100 is not (>= 11 and <= 19) ? 0 : n % 10 is >= 2 and <= 9 && n % 100 is not (>= 11 and <= 19) ? 1 : 2));
        AddRule(["cs", "sk"], n => ((n == 1) ? 0 : n is >= 2 and <= 4 ? 1 : 2));
        AddRule(["csb", "pl"], n => ((n == 1) ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2));
        AddRule(["lv", "prg"], n => (n % 10 == 0 || n % 100 is >= 11 and <= 19 ? 2 : n % 10 == 1 ? 0 : 1));
        AddRule(["blo", "cv", "ksh", "lag"], n => (n == 0 ? 0 : n == 1 ? 1 : 2));
        AddRule(["mnk"], n => (n == 0 ? 0 : n == 1 ? 1 : 2));
        AddRule(["he", "iu", "iw", "naq", "sat", "se", "sma", "smi", "smj", "smn", "sms"], n => (n == 1 ? 0 : n == 2 ? 2 : 1));
        AddRule(["shi"], n => (n <= 1 ? 0 : n <= 10 ? 1 : 2));
        AddRule(["mo", "ro"], n => (n == 1 ? 0 : (n == 0 || (n % 100 > 0 && n % 100 < 20)) ? 1 : 2));
        AddRule(["cy"], n => (n == 0 ? 4 : n == 1 ? 0 : n == 2 ? 1 : n == 3 ? 2 : n == 6 ? 5 : 3));
        AddRule(["gd"], n => ((n == 1 || n == 11) ? 0 : (n == 2 || n == 12) ? 1 : n is > 2 and < 20 ? 2 : 3));
        AddRule(["kw"], GetCornishPluralForm);
        AddRule(["mt"], n => (n == 1 ? 0 : n == 2 ? 4 : n == 0 || n % 100 is >= 3 and <= 10 ? 1 : n % 100 is >= 11 and <= 19 ? 2 : 3));
        AddRule(["dsb", "hsb", "sl"], n => (n % 100 == 1 ? 1 : n % 100 == 2 ? 2 : n % 100 == 3 || n % 100 == 4 ? 3 : 0));
        AddRule(["sgs"], n => (n % 10 == 1 && n % 100 != 11 ? 0 : n == 2 ? 1 : n % 10 is >= 2 and <= 9 && n % 100 is not (>= 11 and <= 19) ? 2 : 3));
        AddRule(["br"], GetBretonPluralForm);
        AddRule(["ga"], n => (n == 1 ? 0 : n == 2 ? 1 : n is > 2 and < 7 ? 2 : n is > 6 and < 11 ? 3 : 4));
        AddRule(["gv"], n => (n % 10 == 1 ? 0 : n % 10 == 2 ? 1 : n % 100 is 0 or 20 or 40 or 60 or 80 ? 2 : 3));
        AddRule(["ar", "ars"], n => (n == 0 ? 0 : n == 1 ? 1 : n == 2 ? 2 : n % 100 >= 3 && n % 100 <= 10 ? 3 : n % 100 >= 11 ? 4 : 5));
    }

    /// <inheritdocs />
    public int Order => 0;

    /// <inheritdocs />
    public bool TryGetRule(CultureInfo culture, out PluralizationRuleDelegate rule)
    {
        // Prefer a specific rule before falling back through the parent cultures.
        while (culture.Name != string.Empty)
        {
            if (s_rules.TryGetValue(culture.Name, out rule))
            {
                return true;
            }

            culture = culture.Parent;
        }

        rule = null;
        return false;
    }

    private static int GetMillionPluralForm(long n, bool includeZero)
    {
        if (n == 1 || (includeZero && n == 0))
        {
            return 0;
        }

        // For integer counts (v = 0, e = 0), 'many' means a nonzero multiple of one million.
        // Keep the existing 'one' (0) and 'other' (1) indexes and append 'many' (2).
        return n != 0 && n % 1_000_000 == 0 ? 2 : 1;
    }

    private static int GetBretonPluralForm(long n)
    {
        var lastTwo = n % 100;
        if (n % 10 == 1 && lastTwo is not (11 or 71 or 91))
        {
            return 0;
        }

        if (n % 10 == 2 && lastTwo is not (12 or 72 or 92))
        {
            return 2;
        }

        if (n % 10 is 3 or 4 or 9 && lastTwo is not (>= 10 and <= 19) and not (>= 70 and <= 79) and not (>= 90 and <= 99))
        {
            return 3;
        }

        return n != 0 && n % 1_000_000 == 0 ? 4 : 1;
    }

    private static int GetCornishPluralForm(long n)
    {
        if (n == 0)
        {
            return 4;
        }

        if (n == 1)
        {
            return 0;
        }

        if (n % 100 is 2 or 22 or 42 or 62 or 82 ||
            (n % 1000 == 0 && n % 100_000 is (>= 1000 and <= 20_000) or 40_000 or 60_000 or 80_000) ||
            n % 1_000_000 == 100_000)
        {
            return 1;
        }

        if (n % 100 is 3 or 23 or 43 or 63 or 83)
        {
            return 2;
        }

        return n % 100 is 1 or 21 or 41 or 61 or 81 ? 5 : 3;
    }

    private static void AddRule(string[] cultures, Func<long, int> rule)
    {
        // CLDR operands use the absolute value. Widen first to support int.MinValue.
        PluralizationRuleDelegate normalizedRule = count => rule(Math.Abs((long)count));
        foreach (var culture in cultures)
        {
            s_rules.Add(culture, normalizedRule);
        }
    }
}
