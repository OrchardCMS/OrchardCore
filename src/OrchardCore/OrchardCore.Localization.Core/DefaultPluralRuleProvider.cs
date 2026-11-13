using System.Globalization;
using static OrchardCore.Localization.PluralCondition;

namespace OrchardCore.Localization;

/// <summary>
/// Provides integer cardinal pluralization rules based on Unicode CLDR 49.
/// See https://www.unicode.org/cldr/charts/49/supplemental/language_plural_rules.html.
/// </summary>
/// <remarks>
/// The index returned by a rule is the <c>msgstr[n]</c> index of the PO files. It doesn't follow the order of the CLDR
/// plural categories. Each rule is described by a <see cref="PluralRuleDescriptor"/>, listing the condition and example
/// counts of each index.
/// </remarks>
public class DefaultPluralRuleProvider : IPluralRuleProvider
{
    private static readonly PluralCondition NonzeroMillions = MultipleOf(1_000_000).Except(Exactly(0));

    private static readonly Dictionary<string, PluralRuleDescriptor> s_rules;
    private static readonly List<PluralRuleDescriptor> s_descriptors;

    static DefaultPluralRuleProvider()
    {
        s_rules = [];
        s_descriptors = [];

        // Retain legacy registrations without explicit CLDR data; do not infer new linguistic rules for them.
        AddRule(["ay", "bm", "bo", "dz", "hnj", "id", "ig", "ii", "in", "ja", "jbo", "jv", "jw", "kde", "kea", "km", "ko", "lkt", "lo", "ms", "my", "nqo", "osa", "sah", "ses", "sg", "su", "th", "to", "tpi", "tt", "wo", "yo", "yue", "zh"], n => 0, All);
        AddRule(["ach", "ak", "am", "arn", "as", "bho", "bn", "csw", "doi", "fa", "ff", "gu", "gun", "guw", "hi", "hy", "kab", "kn", "kok", "kok-Latn", "ln", "mfe", "mg", "mi", "nso", "oc", "pa", "pcm", "si", "tg", "ti", "vi", "wa", "zu"], n => (n > 1 ? 1 : 0), Exactly(0, 1), Remaining);
        AddRule(["af", "an", "anp", "asa", "ast", "az", "bal", "bem", "bez", "bg", "brx", "ce", "cgg", "chr", "ckb", "da", "de", "dv", "ee", "el", "en", "eo", "et", "eu", "fi", "fo", "fur", "fy", "gsw", "ha", "haw", "hne", "hu", "ia", "ie", "io", "jgo", "ji", "jmc", "ka", "kaj", "kcg", "kk", "kkj", "kl", "ks", "ksb", "ku", "ky", "lb", "lg", "lij", "mai", "mas", "mgo", "ml", "mn", "mni", "mr", "nah", "nap", "nb", "nd", "ne", "nl", "nn", "nnh", "no", "nr", "ny", "nyn", "om", "or", "os", "pap", "pms", "ps", "rm", "rof", "rw", "rwk", "saq", "sc", "sco", "sd", "sdh", "seh", "sn", "so", "son", "sq", "ss", "ssy", "st", "sv", "sw", "syr", "ta", "te", "teo", "tig", "tk", "tn", "tr", "ts", "ug", "ur", "uz", "ve", "vo", "vun", "wae", "xh", "xog", "yi"], n => (n != 1 ? 1 : 0), Exactly(1), Remaining);
        AddRule(["tzm"], n => (n is <= 1 or (>= 11 and <= 99) ? 0 : 1), Exactly(0, 1).Or(Between(11, 99)), Remaining);
        AddRule(["fr", "pt", "pt-BR"], n => GetMillionPluralForm(n, includeZero: true), Exactly(0, 1), Remaining, NonzeroMillions);
        AddRule(["ca", "es", "es-AR", "gl", "it", "lld", "pt-PT", "scn", "vec"], n => GetMillionPluralForm(n, includeZero: false), Exactly(1), Remaining, NonzeroMillions);
        AddRule(["is"], n => (n % 10 != 1 || n % 100 == 11 ? 1 : 0), EndsIn(1, 1).Except(EndsIn(2, 11)), Remaining);
        AddRule(["mk"], n => (n % 10 == 1 && n % 100 != 11 ? 0 : 1), EndsIn(1, 1).Except(EndsIn(2, 11)), Remaining);
        AddRule(["ceb", "fil", "tl"], n => (n % 10 is 4 or 6 or 9 ? 1 : 0), Remaining, EndsIn(1, 4, 6, 9));
        AddRule(["be", "bs", "hr", "ru", "sh", "sr", "uk"], n => (n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2), EndsIn(1, 1).Except(EndsIn(2, 11)), EndsInRange(1, 2, 4).Except(EndsInRange(2, 12, 14)), Remaining);
        AddRule(["lt"], n => (n % 10 == 1 && n % 100 is not (>= 11 and <= 19) ? 0 : n % 10 is >= 2 and <= 9 && n % 100 is not (>= 11 and <= 19) ? 1 : 2), EndsIn(1, 1).Except(EndsInRange(2, 11, 19)), EndsInRange(1, 2, 9).Except(EndsInRange(2, 11, 19)), Remaining);
        AddRule(["cs", "sk"], n => ((n == 1) ? 0 : n is >= 2 and <= 4 ? 1 : 2), Exactly(1), Between(2, 4), Remaining);
        AddRule(["csb", "pl"], n => ((n == 1) ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2), Exactly(1), EndsInRange(1, 2, 4).Except(EndsInRange(2, 12, 14)), Remaining);
        AddRule(["lv", "prg"], n => (n % 10 == 0 || n % 100 is >= 11 and <= 19 ? 2 : n % 10 == 1 ? 0 : 1), EndsIn(1, 1).Except(EndsIn(2, 11)), Remaining, EndsIn(1, 0).Or(EndsInRange(2, 11, 19)));
        AddRule(["blo", "cv", "ksh", "lag"], n => (n == 0 ? 0 : n == 1 ? 1 : 2), Exactly(0), Exactly(1), Remaining);
        AddRule(["mnk"], n => (n == 0 ? 0 : n == 1 ? 1 : 2), Exactly(0), Exactly(1), Remaining);
        AddRule(["he", "iu", "iw", "naq", "sat", "se", "sma", "smi", "smj", "smn", "sms"], n => (n == 1 ? 0 : n == 2 ? 2 : 1), Exactly(1), Remaining, Exactly(2));
        AddRule(["shi"], n => (n <= 1 ? 0 : n <= 10 ? 1 : 2), Exactly(0, 1), Between(2, 10), Remaining);
        AddRule(["mo", "ro"], n => (n == 1 ? 0 : (n == 0 || (n % 100 > 0 && n % 100 < 20)) ? 1 : 2), Exactly(1), Exactly(0).Or(EndsInRange(2, 1, 19)).Except(Exactly(1)), Remaining);
        AddRule(["cy"], n => (n == 0 ? 4 : n == 1 ? 0 : n == 2 ? 1 : n == 3 ? 2 : n == 6 ? 5 : 3), Exactly(1), Exactly(2), Exactly(3), Remaining, Exactly(0), Exactly(6));
        AddRule(["gd"], n => ((n == 1 || n == 11) ? 0 : (n == 2 || n == 12) ? 1 : n is > 2 and < 20 ? 2 : 3), Exactly(1, 11), Exactly(2, 12), Between(3, 19).Except(Exactly(11, 12)), Remaining);
        AddRule(["kw"], GetCornishPluralForm, Exactly(1), EndsIn(2, 2, 22, 42, 62, 82).Or(EndsIn(5, 40_000, 60_000, 80_000)).Or(EndsIn(6, 100_000)).Or(EndsInRange(5, 1_000, 20_000).And(MultipleOf(1_000))), EndsIn(2, 3, 23, 43, 63, 83), Remaining, Exactly(0), EndsIn(2, 1, 21, 41, 61, 81).Except(Exactly(1)));
        AddRule(["mt"], n => (n == 1 ? 0 : n == 2 ? 4 : n == 0 || n % 100 is >= 3 and <= 10 ? 1 : n % 100 is >= 11 and <= 19 ? 2 : 3), Exactly(1), Exactly(0).Or(EndsInRange(2, 3, 10)), EndsInRange(2, 11, 19), Remaining, Exactly(2));
        AddRule(["dsb", "hsb", "sl"], n => (n % 100 == 1 ? 1 : n % 100 == 2 ? 2 : n % 100 == 3 || n % 100 == 4 ? 3 : 0), Remaining, EndsIn(2, 1), EndsIn(2, 2), EndsIn(2, 3, 4));
        AddRule(["sgs"], n => (n % 10 == 1 && n % 100 != 11 ? 0 : n == 2 ? 1 : n % 10 is >= 2 and <= 9 && n % 100 is not (>= 11 and <= 19) ? 2 : 3), EndsIn(1, 1).Except(EndsIn(2, 11)), Exactly(2), EndsInRange(1, 2, 9).Except(EndsInRange(2, 11, 19)).Except(Exactly(2)), Remaining);
        AddRule(["br"], GetBretonPluralForm, EndsIn(1, 1).Except(EndsIn(2, 11, 71, 91)), Remaining, EndsIn(1, 2).Except(EndsIn(2, 12, 72, 92)), EndsIn(1, 3, 4, 9).Except(EndsInRange(2, 10, 19).Or(EndsInRange(2, 70, 79)).Or(EndsInRange(2, 90, 99))), NonzeroMillions);
        AddRule(["ga"], n => (n == 1 ? 0 : n == 2 ? 1 : n is > 2 and < 7 ? 2 : n is > 6 and < 11 ? 3 : 4), Exactly(1), Exactly(2), Between(3, 6), Between(7, 10), Remaining);
        AddRule(["gv"], n => (n % 10 == 1 ? 0 : n % 10 == 2 ? 1 : n % 100 is 0 or 20 or 40 or 60 or 80 ? 2 : 3), EndsIn(1, 1), EndsIn(1, 2), EndsIn(2, 0, 20, 40, 60, 80), Remaining);
        AddRule(["ar", "ars"], n => (n == 0 ? 0 : n == 1 ? 1 : n == 2 ? 2 : n % 100 >= 3 && n % 100 <= 10 ? 3 : n % 100 >= 11 ? 4 : 5), Exactly(0), Exactly(1), Exactly(2), EndsInRange(2, 3, 10), EndsInRange(2, 11, 99), Remaining);
    }

    /// <summary>
    /// Gets the descriptors of all the rules, in the order they are declared.
    /// </summary>
    public static IReadOnlyList<PluralRuleDescriptor> Rules => s_descriptors;

    /// <inheritdocs />
    public int Order => 0;

    /// <inheritdocs />
    public bool TryGetRule(CultureInfo culture, out PluralizationRuleDelegate rule)
    {
        if (TryGetRuleDescriptor(culture, out var descriptor))
        {
            rule = descriptor.Rule;

            return true;
        }

        rule = null;

        return false;
    }

    /// <summary>
    /// Gets the descriptor of the rule used for a culture: the rule of the culture itself or, if it has none, the rule
    /// of its closest parent culture.
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <param name="descriptor">The descriptor of the rule, if any.</param>
    /// <returns><see langword="true"/> if a rule exists for the culture; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetRuleDescriptor(CultureInfo culture, out PluralRuleDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(culture);

        // Prefer a specific rule before falling back through the parent cultures.
        while (culture.Name != string.Empty)
        {
            if (s_rules.TryGetValue(culture.Name, out descriptor))
            {
                return true;
            }

            culture = culture.Parent;
        }

        descriptor = null;
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

    // The conditions describe the forms in the order of the indexes returned by the rule: msgstr[0], msgstr[1], ...
    private static void AddRule(string[] cultures, Func<long, int> rule, params PluralCondition[] conditions)
    {
        // CLDR operands use the absolute value. Widen first to support int.MinValue.
        PluralizationRuleDelegate normalizedRule = count => rule(Math.Abs((long)count));

        var descriptor = new PluralRuleDescriptor(cultures, normalizedRule, conditions);

        s_descriptors.Add(descriptor);

        foreach (var culture in cultures)
        {
            s_rules.Add(culture, descriptor);
        }
    }
}
