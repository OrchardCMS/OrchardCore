using System.Text;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class PluralRuleDescriptorTests
{
    private const string DocsStartMarker = "<!-- plural-rules:start -->";
    private const string DocsEndMarker = "<!-- plural-rules:end -->";

    public static TheoryData<int> RuleIndexes()
    {
        var data = new TheoryData<int>();

        for (var i = 0; i < DefaultPluralRuleProvider.Rules.Count; i++)
        {
            data.Add(i);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(RuleIndexes))]
    public void Conditions_ValidationCounts_MatchRuntimeRule(int ruleIndex)
    {
        var descriptor = DefaultPluralRuleProvider.Rules[ruleIndex];

        foreach (var count in GetValidationCounts())
        {
            var described = ResolveFormIndex(descriptor, count);
            var actual = descriptor.Rule(count);

            Assert.True(
                described == actual,
                $"The descriptions of [{string.Join(", ", descriptor.Cultures)}] select msgstr[{described}] for {count}, but the rule returns {actual}.");
        }
    }

    [Theory]
    [MemberData(nameof(RuleIndexes))]
    public void Forms_Default_AreOrderedByRuntimeIndexAndAllReachable(int ruleIndex)
    {
        var descriptor = DefaultPluralRuleProvider.Rules[ruleIndex];

        Assert.Equal(Enumerable.Range(0, descriptor.FormCount), descriptor.Forms.Select(f => f.Index));
        Assert.Single(descriptor.Forms, f => f.Condition.IsRemaining);

        var returned = GetValidationCounts().Select(n => descriptor.Rule(n)).Distinct().Order();

        Assert.Equal(Enumerable.Range(0, descriptor.FormCount), returned);
    }

    [Theory]
    [MemberData(nameof(RuleIndexes))]
    public void Examples_Default_AreSelectedByRuntimeRule(int ruleIndex)
    {
        var descriptor = DefaultPluralRuleProvider.Rules[ruleIndex];

        foreach (var form in descriptor.Forms)
        {
            Assert.NotEmpty(form.Examples);
            Assert.True(form.Examples.Count <= PluralRuleDescriptor.ExampleCount);
            Assert.Equal(form.Examples.Order(), form.Examples);
            Assert.All(form.Examples, example => Assert.Equal(form.Index, descriptor.Rule(example)));
        }
    }

    [Fact]
    public void Cultures_Default_ShareRuleWithTryGetRule()
    {
        var provider = new DefaultPluralRuleProvider();

        foreach (var descriptor in DefaultPluralRuleProvider.Rules)
        {
            foreach (var culture in descriptor.Cultures)
            {
                var cultureInfo = CultureInfo.GetCultureInfo(culture);

                Assert.True(DefaultPluralRuleProvider.TryGetRuleDescriptor(cultureInfo, out var found));
                Assert.Same(descriptor, found);
                Assert.True(provider.TryGetRule(cultureInfo, out var rule));
                Assert.Same(descriptor.Rule, rule);
            }
        }
    }

    [Fact]
    public void TryGetRuleDescriptor_Polish_DescribesEachIndex()
    {
        Assert.True(DefaultPluralRuleProvider.TryGetRuleDescriptor(CultureInfo.GetCultureInfo("pl-PL"), out var descriptor));
        Assert.Equal(3, descriptor.FormCount);

        Assert.Equal("Exactly 1", descriptor.Forms[0].Description);
        Assert.Equal([1], descriptor.Forms[0].Examples);

        Assert.Equal("Ends in 2-4, except 12-14", descriptor.Forms[1].Description);
        Assert.Equal([2, 4, 22, 24], descriptor.Forms[1].Examples);

        Assert.Equal("All remaining counts", descriptor.Forms[2].Description);
        Assert.Equal([0, 5, 21, 25], descriptor.Forms[2].Examples);
    }

    [Fact]
    public void TryGetRuleDescriptor_French_DescribesMillionMultiples()
    {
        Assert.True(DefaultPluralRuleProvider.TryGetRuleDescriptor(CultureInfo.GetCultureInfo("fr-CA"), out var descriptor));
        Assert.Equal(3, descriptor.FormCount);

        Assert.Equal("Exactly 0 or 1", descriptor.Forms[0].Description);
        Assert.Equal("All remaining counts", descriptor.Forms[1].Description);
        Assert.Equal("Multiple of 1,000,000, except 0", descriptor.Forms[2].Description);
        Assert.Equal([1_000_000, 2_000_000, 3_000_000, 4_000_000], descriptor.Forms[2].Examples);
    }

    [Theory]
    [InlineData("sl", 0, "All remaining counts")]
    [InlineData("sl", 1, "Ends in 01")]
    [InlineData("lv", 0, "Ends in 1, except 11")]
    [InlineData("lv", 2, "Ends in 0 or 11-19")]
    [InlineData("ro", 1, "Exactly 0, or ends in 01-19, except exactly 1")]
    [InlineData("gd", 2, "From 3 to 19, except 11 or 12")]
    [InlineData("cy", 4, "Exactly 0")]
    [InlineData("cy", 5, "Exactly 6")]
    [InlineData("mt", 4, "Exactly 2")]
    [InlineData("he", 2, "Exactly 2")]
    [InlineData("fil", 0, "All remaining counts")]
    [InlineData("br", 3, "Ends in 3, 4 or 9, except 10-19, 70-79 or 90-99")]
    [InlineData("sgs", 2, "Ends in 2-9, except 11-19, and except exactly 2")]
    [InlineData("kw", 1, "Ends in 02, 22, 42, 62, 82, 40000, 60000, 80000 or 100000, or ends in 01000-20000 and multiple of 1,000")]
    [InlineData("kw", 5, "Ends in 01, 21, 41, 61 or 81, except exactly 1")]
    [InlineData("ja", 0, "All counts")]
    public void TryGetRuleDescriptor_IndexNotFollowingCldrOrder_DescribesRuntimeIndex(string culture, int index, string expected)
    {
        Assert.True(DefaultPluralRuleProvider.TryGetRuleDescriptor(CultureInfo.GetCultureInfo(culture), out var descriptor));
        Assert.Equal(expected, descriptor.Forms[index].Description);
    }

    [Theory]
    [InlineData("pt-BR", "pt")]
    [InlineData("pt-AO", "pt")]
    [InlineData("pt-PT", "pt-PT")]
    [InlineData("es-MX", "es")]
    [InlineData("kok-Latn-IN", "kok-Latn")]
    public void TryGetRuleDescriptor_SpecificCulture_UsesClosestRegisteredCulture(string culture, string registered)
    {
        Assert.True(DefaultPluralRuleProvider.TryGetRuleDescriptor(CultureInfo.GetCultureInfo(culture), out var descriptor));
        Assert.Contains(registered, descriptor.Cultures);
    }

    [Fact]
    public void TryGetRuleDescriptor_PortugalAndPortuguese_DescribeDifferentZeroForms()
    {
        Assert.True(DefaultPluralRuleProvider.TryGetRuleDescriptor(CultureInfo.GetCultureInfo("pt"), out var portuguese));
        Assert.True(DefaultPluralRuleProvider.TryGetRuleDescriptor(CultureInfo.GetCultureInfo("pt-PT"), out var portugal));

        Assert.NotSame(portuguese, portugal);
        Assert.Equal("Exactly 0 or 1", portuguese.Forms[0].Description);
        Assert.Equal("Exactly 1", portugal.Forms[0].Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("zxx")]
    public void TryGetRuleDescriptor_UnsupportedCulture_ReturnsFalse(string culture)
    {
        Assert.False(DefaultPluralRuleProvider.TryGetRuleDescriptor(CultureInfo.GetCultureInfo(culture), out var descriptor));
        Assert.Null(descriptor);
    }

    [Fact]
    public void Docs_PluralRules_MatchDescriptors()
    {
        var path = Path.Combine(FindRepositoryRoot(), "src", "docs", "reference", "modules", "Localize", "README.md");
        var content = File.ReadAllText(path).ReplaceLineEndings("\n");

        var start = content.IndexOf(DocsStartMarker, StringComparison.Ordinal);
        var end = content.IndexOf(DocsEndMarker, StringComparison.Ordinal);

        Assert.True(start >= 0 && end > start, $"The '{DocsStartMarker}' and '{DocsEndMarker}' markers were not found in '{path}'.");

        var expected = RenderMarkdown();
        var actual = content[(start + DocsStartMarker.Length)..end];

        if (actual != expected && System.Environment.GetEnvironmentVariable("ORCHARDCORE_UPDATE_DOCS") == "1")
        {
            File.WriteAllText(path, string.Concat(content.AsSpan(0, start + DocsStartMarker.Length), expected, content.AsSpan(end)));

            return;
        }

        Assert.True(actual == expected, $"The plural rules of '{path}' are outdated. Run this test with the ORCHARDCORE_UPDATE_DOCS=1 environment variable to update them.");
    }

    // Every count below 120,000, covering all the endings of up to five digits, the multiples of 1,000 below 10,000,000,
    // the multiples of 1,000,000, the extreme values, and the negation of all of them.
    private static IEnumerable<int> GetValidationCounts()
    {
        var counts = new List<int>();

        for (var count = 0; count < 120_000; count++)
        {
            counts.Add(count);
        }

        for (var count = 120_000; count < 10_000_000; count += 1_000)
        {
            counts.Add(count);
        }

        for (long count = 10_000_000; count <= int.MaxValue; count += 1_000_000)
        {
            counts.Add((int)count);
        }

        counts.Add(int.MaxValue);

        foreach (var count in counts)
        {
            yield return count;
            yield return -count;
        }

        yield return int.MinValue;
    }

    private static int ResolveFormIndex(PluralRuleDescriptor descriptor, int count)
    {
        PluralFormDescriptor match = null;

        foreach (var form in descriptor.Forms)
        {
            if (form.Condition.IsMatch(count))
            {
                Assert.True(match is null, $"The descriptions of [{string.Join(", ", descriptor.Cultures)}] overlap for {count}: {match?.Description} / {form.Description}.");

                match = form;
            }
        }

        return (match ?? descriptor.Forms.Single(f => f.Condition.IsRemaining)).Index;
    }

    private static string RenderMarkdown()
    {
        var builder = new StringBuilder();

        foreach (var descriptor in DefaultPluralRuleProvider.Rules)
        {
            builder.Append('\n');
            builder.Append($"**{descriptor.FormCount} {(descriptor.FormCount == 1 ? "form" : "forms")}:** ");
            builder.Append(string.Join(", ", descriptor.Cultures.Select(c => $"`{c}`")));
            builder.Append("\n\n");
            builder.Append("| Translation | Condition | Examples |\n");
            builder.Append("| --- | --- | --- |\n");

            foreach (var form in descriptor.Forms)
            {
                var examples = string.Join(", ", form.Examples.Select(e => e.ToString("N0", CultureInfo.InvariantCulture)));

                builder.Append($"| `msgstr[{form.Index}]` | {form.Description} | {examples} |\n");
            }
        }

        builder.Append('\n');

        return builder.ToString();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OrchardCore.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        return directory.FullName;
    }
}
