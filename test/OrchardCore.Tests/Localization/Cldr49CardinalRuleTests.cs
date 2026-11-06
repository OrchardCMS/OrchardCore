using System.Text.RegularExpressions;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class Cldr49CardinalRuleTests
{
    // Predicates and locale lists derived from Unicode CLDR 49 common/supplemental/plurals.xml.
    // Copyright © 1991-2026 Unicode, Inc. SPDX-License-Identifier: Unicode-3.0.
    // https://www.unicode.org/cldr/charts/49/supplemental/language_plural_rules.html
    // Columns: locales | OrchardCore translation-index categories | original CLDR predicates.
    private const string Rules = """
        bm bo dz hnj id ig ii in ja jbo jv jw kde kea km ko lkt lo ms my nqo osa sah ses sg su th to tpi wo yo yue zh|other|other:
        am as bn doi fa gu hi kn kok kok_Latn pcm tg vi zu|one other|one:i = 0 or n = 1;other:
        ff hy kab|one other|one:i = 0,1;other:
        ast de en et fi fy ia ie io ji lij nl sc sv sw ur yi|one other|one:i = 1 and v = 0;other:
        si|one other|one:n = 0,1 or i = 0 and f = 1;other:
        ak bho csw guw ln mg nso pa ti wa|one other|one:n = 0..1;other:
        tzm|one other|one:n = 0..1 or n = 11..99;other:
        af an asa az bal bem bez bg brx ce cgg chr ckb dv ee el eo eu fo fur gsw ha haw hu jgo jmc ka kaj kcg kk kkj kl ks ksb ku ky lb lg mas mgo ml mn mr nah nb nd ne nn nnh no nr ny nyn om or os pap ps rm rof rwk saq sd sdh seh sn so sq ss ssy st syr ta te teo tig tk tn tr ts ug uz ve vo vun wae xh xog|one other|one:n = 1;other:
        da|one other|one:n = 1 or t != 0 and i = 0,1;other:
        is|one other|one:t = 0 and i % 10 = 1 and i % 100 != 11 or t % 10 = 1 and t % 100 != 11;other:
        mk|one other|one:v = 0 and i % 10 = 1 and i % 100 != 11 or f % 10 = 1 and f % 100 != 11;other:
        ceb fil tl|one other|one:v = 0 and i = 1,2,3 or v = 0 and i % 10 != 4,6,9 or v != 0 and f % 10 != 4,6,9;other:
        lv prg|one other zero|zero:n % 10 = 0 or n % 100 = 11..19 or v = 2 and f % 100 = 11..19;one:n % 10 = 1 and n % 100 != 11 or v = 2 and f % 10 = 1 and f % 100 != 11 or v != 2 and f % 10 = 1;other:
        lag|zero one other|zero:n = 0;one:i = 0,1 and n != 0;other:
        blo cv ksh|zero one other|zero:n = 0;one:n = 1;other:
        he iw|one other two|one:i = 1 and v = 0 or i = 0 and v != 0;two:i = 2 and v = 0;other:
        iu naq sat se sma smi smj smn sms|one other two|one:n = 1;two:n = 2;other:
        shi|one few other|one:i = 0 or n = 1;few:n = 2..10;other:
        mo ro|one few other|one:i = 1 and v = 0;few:v != 0 or n = 0 or n != 1 and n % 100 = 1..19;other:
        bs hr sh sr|one few other|one:v = 0 and i % 10 = 1 and i % 100 != 11 or f % 10 = 1 and f % 100 != 11;few:v = 0 and i % 10 = 2..4 and i % 100 != 12..14 or f % 10 = 2..4 and f % 100 != 12..14;other:
        fr pt|one other many|one:i = 0..1;many:e = 0 and i != 0 and i % 1000000 = 0 and v = 0 or e != 0..5;other:
        ca gl it lld pt_PT scn vec|one other many|one:i = 1 and v = 0;many:e = 0 and i != 0 and i % 1000000 = 0 and v = 0 or e != 0..5;other:
        es|one other many|one:n = 1;many:e = 0 and i != 0 and i % 1000000 = 0 and v = 0 or e != 0..5;other:
        gd|one two few other|one:n = 1,11;two:n = 2,12;few:n = 3..10,13..19;other:
        sl|other one two few|one:v = 0 and i % 100 = 1;two:v = 0 and i % 100 = 2;few:v = 0 and i % 100 = 3..4 or v != 0;other:
        dsb hsb|other one two few|one:v = 0 and i % 100 = 1 or f % 100 = 1;two:v = 0 and i % 100 = 2 or f % 100 = 2;few:v = 0 and i % 100 = 3..4 or f % 100 = 3..4;other:
        cs sk|one few other|one:i = 1 and v = 0;few:i = 2..4 and v = 0;many:v != 0;other:
        pl|one few many|one:i = 1 and v = 0;few:v = 0 and i % 10 = 2..4 and i % 100 != 12..14;many:v = 0 and i != 1 and i % 10 = 0..1 or v = 0 and i % 10 = 5..9 or v = 0 and i % 100 = 12..14;other:
        be|one few many|one:n % 10 = 1 and n % 100 != 11;few:n % 10 = 2..4 and n % 100 != 12..14;many:n % 10 = 0 or n % 10 = 5..9 or n % 100 = 11..14;other:
        lt|one few other|one:n % 10 = 1 and n % 100 != 11..19;few:n % 10 = 2..9 and n % 100 != 11..19;many:f != 0;other:
        ru uk|one few many|one:v = 0 and i % 10 = 1 and i % 100 != 11;few:v = 0 and i % 10 = 2..4 and i % 100 != 12..14;many:v = 0 and i % 10 = 0 or v = 0 and i % 10 = 5..9 or v = 0 and i % 100 = 11..14;other:
        sgs|one two few other|one:n % 10 = 1 and n % 100 != 11;two:n = 2;few:n != 2 and n % 10 = 2..9 and n % 100 != 11..19;many:f != 0;other:
        br|one other two few many|one:n % 10 = 1 and n % 100 != 11,71,91;two:n % 10 = 2 and n % 100 != 12,72,92;few:n % 10 = 3..4,9 and n % 100 != 10..19,70..79,90..99;many:n != 0 and n % 1000000 = 0;other:
        mt|one few many other two|one:n = 1;two:n = 2;few:n = 0 or n % 100 = 3..10;many:n % 100 = 11..19;other:
        ga|one two few many other|one:n = 1;two:n = 2;few:n = 3..6;many:n = 7..10;other:
        gv|one two few other|one:v = 0 and i % 10 = 1;two:v = 0 and i % 10 = 2;few:v = 0 and i % 100 = 0,20,40,60,80;many:v != 0;other:
        kw|one two few other zero many|zero:n = 0;one:n = 1;two:n % 100 = 2,22,42,62,82 or n % 1000 = 0 and n % 100000 = 1000..20000,40000,60000,80000 or n != 0 and n % 1000000 = 100000;few:n % 100 = 3,23,43,63,83;many:n != 1 and n % 100 = 1,21,41,61,81;other:
        ar ars|zero one two few many other|zero:n = 0;one:n = 1;two:n = 2;few:n % 100 = 3..10;many:n % 100 = 11..99;other:
        cy|one two few other zero many|zero:n = 0;one:n = 1;two:n = 2;few:n = 3;many:n = 6;other:
        """;

    public static IEnumerable<object[]> CardinalRules()
    {
        foreach (var line in Rules.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var columns = line.Split('|');
            foreach (var culture in columns[0].Split(' '))
            {
                yield return [culture.Replace('_', '-'), columns[1], columns[2]];
            }
        }
    }

    [Theory]
    [MemberData(nameof(CardinalRules))]
    public void TryGetRule_Cldr49IntegerCardinals_SelectsExpectedCategory(string culture, string indexCategories, string predicates)
    {
        var provider = new DefaultPluralRuleProvider();
        Assert.True(provider.TryGetRule(CultureInfo.GetCultureInfo(culture), out var rule));
        var categories = indexCategories.Split(' ');
        var rules = predicates.Split(';').Select(predicate =>
        {
            var parts = predicate.Split(':', 2);
            return (Category: parts[0], Matches: CompileCondition(parts[1]));
        }).ToArray();
        var dictionary = new CultureDictionary(culture, rule);
        var key = new CultureDictionaryRecordKey { MessageId = "item" };
        dictionary.Translations[key] = categories;

        foreach (var count in Counts())
        {
            var magnitude = Math.Abs((long)count);
            var category = rules.First(candidate => candidate.Matches(magnitude)).Category;
            var expected = Array.IndexOf(categories, category);
            Assert.True(expected >= 0, $"{culture}: integer {count} unexpectedly selected {category}.");
            Assert.True(rule(count) == expected, $"{culture}: {count} expected {category} (index {expected}), got index {rule(count)}.");
            Assert.Equal(category, dictionary[key, count]);
        }
    }

    [Theory]
    [InlineData("he", 2, 2)]
    [InlineData("fr", 1_000_000, 2)]
    [InlineData("br", 1_000_000, 4)]
    [InlineData("mt", 2, 4)]
    [InlineData("cy", 0, 4)]
    [InlineData("kw", 21, 5)]
    public void Dictionary_MissingNewCategory_RequiresCatalogMigration(string culture, int count, int index)
    {
        var provider = new DefaultPluralRuleProvider();
        Assert.True(provider.TryGetRule(CultureInfo.GetCultureInfo(culture), out var rule));
        var dictionary = new CultureDictionary(culture, rule);
        var key = new CultureDictionaryRecordKey { MessageId = "item" };
        dictionary.Translations[key] = new string[index];

        Assert.Throws<PluralFormNotFoundException>(() => dictionary[key, count]);
    }

    private static IEnumerable<int> Counts()
    {
        var counts = Enumerable.Range(0, 1001).ToHashSet();
        for (var n = 1000; n <= 200_000; n += 1000)
        {
            counts.UnionWith([n - 1, n, n + 1]);
        }

        for (var n = 1_000_000; n <= 3_000_000; n += 100_000)
        {
            counts.UnionWith([n - 1, n, n + 1]);
        }

        counts.UnionWith([999_999, 1_000_001, 1_000_000_000, 2_000_000_000, 2_147_000_000, int.MaxValue]);
        foreach (var count in counts)
        {
            yield return count;
            if (count != 0)
            {
                yield return -count;
            }
        }

        yield return int.MinValue;
    }

    private static Func<long, bool> CompileCondition(string condition)
    {
        if (condition.Length == 0)
        {
            return _ => true;
        }

        var alternatives = condition.Split(" or ").Select(alternative =>
            alternative.Split(" and ").Select(CompileRelation).ToArray()).ToArray();
        return n => alternatives.Any(relations => relations.All(relation => relation(n)));
    }

    private static Func<long, bool> CompileRelation(string relation)
    {
        var match = Regex.Match(relation, @"^(n|i|v|f|t|e)(?: % (\d+))? (!?=) (.+)$");
        Assert.True(match.Success, $"Unsupported CLDR relation: {relation}");
        var integerOperand = match.Groups[1].Value is "n" or "i";
        var modulus = match.Groups[2].Success ? long.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) : 0;
        var negate = match.Groups[3].Value == "!=";
        var ranges = match.Groups[4].Value.Split(',').Select(range =>
        {
            var bounds = range.Split("..");
            return (Min: long.Parse(bounds[0], CultureInfo.InvariantCulture), Max: long.Parse(bounds[^1], CultureInfo.InvariantCulture));
        }).ToArray();
        return n =>
        {
            var operand = integerOperand ? n : 0;
            if (modulus != 0)
            {
                operand %= modulus;
            }

            return ranges.Any(range => operand >= range.Min && operand <= range.Max) != negate;
        };
    }
}
