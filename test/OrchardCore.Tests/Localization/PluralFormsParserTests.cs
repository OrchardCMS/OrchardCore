using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;

namespace OrchardCore.Tests.Localization;

public class PluralFormsParserTests
{
    private const string Czech = "nplurals=3; plural=(n==1) ? 0 : (n>=2 && n<=4) ? 1 : 2;";
    private const string Polish = "nplurals=3; plural=(n==1 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2);";
    private const string Arabic = "nplurals=6; plural=(n==0 ? 0 : n==1 ? 1 : n==2 ? 2 : n%100>=3 && n%100<=10 ? 3 : n%100>=11 ? 4 : 5);";
    private const string Russian = "nplurals=3; plural=(n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2);";

    public static TheoryData<string, PluralizationRuleDelegate> KnownRules => new()
    {
        { Czech, PluralizationRule.Czech },
        { Arabic, PluralizationRule.Arabic },
        { Russian, PluralizationRule.Russian },
        { "nplurals=2; plural=n != 1;", PluralizationRule.English },
        { "nplurals=2; plural=(n != 1);", PluralizationRule.English },
    };

    [Theory]
    [MemberData(nameof(KnownRules))]
    public void Parse_KnownRule_MatchesReferenceRule(string pluralForms, PluralizationRuleDelegate expected)
    {
        var rule = PluralFormsParser.Parse(pluralForms);

        Assert.NotNull(rule);

        for (var n = 0; n <= 1000; n++)
        {
            Assert.Equal(expected(n), rule(n));
        }
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(4, 1)]
    [InlineData(5, 2)]
    [InlineData(12, 2)]
    [InlineData(22, 1)]
    [InlineData(25, 2)]
    [InlineData(112, 2)]
    [InlineData(122, 1)]
    public void Parse_PolishRule_ReturnsExpectedForm(int count, int expected)
    {
        Assert.True(PluralFormsParser.TryParse(Polish, out var pluralCount, out var rule));

        Assert.Equal(3, pluralCount);
        Assert.Equal(expected, rule(count));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(1000000, 1)]
    public void Parse_GreaterThanOneRule_ReturnsExpectedForm(int count, int expected)
    {
        var rule = PluralFormsParser.Parse("nplurals=2; plural=(n > 1);");

        Assert.Equal(expected, rule(count));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    public void Parse_SingleFormRule_ReturnsFirstForm(int count)
    {
        Assert.True(PluralFormsParser.TryParse("nplurals=1; plural=0;", out var pluralCount, out var rule));

        Assert.Equal(1, pluralCount);
        Assert.Equal(0, rule(count));
    }

    [Theory]
    [InlineData("nplurals=2; plural=1 + 2 * 3 == 7;", 1)]
    [InlineData("nplurals=2; plural=(1 + 2) * 3 == 9;", 1)]
    [InlineData("nplurals=2; plural=10 - 4 - 3 == 3;", 1)]
    [InlineData("nplurals=2; plural=100 / 10 / 5 == 2;", 1)]
    [InlineData("nplurals=2; plural=!0;", 1)]
    [InlineData("nplurals=2; plural=!!5;", 1)]
    [InlineData("nplurals=2; plural=!(n == 3);", 0)]
    [InlineData("nplurals=2; plural=0 || 0 && 1;", 0)]
    [InlineData("nplurals=2; plural=1 || 0 && 0;", 1)]
    [InlineData("nplurals=3; plural=n == 3 ? 2 : n == 2 ? 1 : 0;", 2)]
    [InlineData("nplurals=3; plural=1 ? 0 ? 1 : 2 : 0;", 2)]
    [InlineData("nplurals=2; plural=n <= 3 && n >= 3 && n < 4 && n > 2;", 1)]
    [InlineData("  nplurals = 2 ;  plural = ( n != 3 ) ;  ", 0)]
    [InlineData("nplurals=2; plural=n", 0)]
    public void Parse_Expression_HonorsPrecedenceAndAssociativity(string pluralForms, int expected)
    {
        var rule = PluralFormsParser.Parse(pluralForms);

        Assert.NotNull(rule);
        Assert.Equal(expected, rule(3));
    }

    [Theory]
    [InlineData("nplurals=2; plural=n / 0;")]
    [InlineData("nplurals=2; plural=n % 0;")]
    [InlineData("nplurals=2; plural=n % (n - n);")]
    public void Parse_DivisionByZero_ReturnsFirstForm(string pluralForms)
    {
        var rule = PluralFormsParser.Parse(pluralForms);

        Assert.NotNull(rule);
        Assert.Equal(0, rule(5));
    }

    [Theory]
    [InlineData("nplurals=2; plural=5;")]
    [InlineData("nplurals=2; plural=n - 10;")]
    public void Parse_FormOutOfRange_ReturnsFirstForm(string pluralForms)
    {
        var rule = PluralFormsParser.Parse(pluralForms);

        Assert.NotNull(rule);
        Assert.Equal(0, rule(3));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("plural=n != 1;")]
    [InlineData("nplurals=2;")]
    [InlineData("nplurals=0; plural=0;")]
    [InlineData("nplurals=-1; plural=0;")]
    [InlineData("nplurals=x; plural=0;")]
    [InlineData("nplurals=101; plural=0;")]
    [InlineData("nplurals=2; plural=;")]
    [InlineData("nplurals=2; plural=n !=;")]
    [InlineData("nplurals=2; plural=(n != 1;")]
    [InlineData("nplurals=2; plural=n != 1);")]
    [InlineData("nplurals=2; plural=n = 1;")]
    [InlineData("nplurals=2; plural=n & 1;")]
    [InlineData("nplurals=2; plural=n | 1;")]
    [InlineData("nplurals=2; plural=n ? 1;")]
    [InlineData("nplurals=2; plural=-n;")]
    [InlineData("nplurals=2; plural=x != 1;")]
    [InlineData("nplurals=2; plural=n != 1.5;")]
    [InlineData("nplurals=2; plural=n != 99999999999999999999;")]
    [InlineData("nplurals=2; plural=n != 1; extra=1;")]
    [InlineData("nplurals=2; garbage; plural=n != 1;")]
    public void Parse_InvalidValue_ReturnsNull(string pluralForms)
    {
        Assert.Null(PluralFormsParser.Parse(pluralForms));
        Assert.False(PluralFormsParser.TryParse(pluralForms, out var pluralCount, out var rule));
        Assert.Equal(0, pluralCount);
        Assert.Null(rule);
    }

    [Fact]
    public void Parse_DeeplyNestedExpression_ReturnsNull()
    {
        var pluralForms = $"nplurals=2; plural={new string('(', 200)}n{new string(')', 200)};";

        Assert.Null(PluralFormsParser.Parse(pluralForms));
    }

    [Fact]
    public void Parse_TooLongExpression_ReturnsNull()
    {
        var pluralForms = $"nplurals=2; plural={string.Join(" + ", Enumerable.Repeat("n", 1000))};";

        Assert.Null(PluralFormsParser.Parse(pluralForms));
    }
}
