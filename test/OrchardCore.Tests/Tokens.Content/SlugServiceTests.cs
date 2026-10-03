using OrchardCore.Localization;
using OrchardCore.Modules.Services;

namespace OrchardCore.Tests.Tokens.Content;

public class SlugServiceTests
{
    private readonly SlugService _slugService;

    public SlugServiceTests()
    {
        _slugService = new SlugService();
    }

    [Theory]
    [InlineData("a - b", "a-b")]
    [InlineData("a  -  -      -  -   -   -b", "a-b")]
    [InlineData("a - b - c-- d", "a-b-c-d")]
    public void Strip_ContiguousDashes_Succeeds(string input, string expected)
    {
        var slug = _slugService.Slugify(input);
        Assert.Equal(expected, slug);
    }

    [Fact]
    public void Change_PercentSymbolsToHyphans_Succeeds()
    {
        var slug = _slugService.Slugify("a%d");
        Assert.Equal("a-d", slug);
    }

    [Fact]
    public void Change_DotSymbolsToHyphans_Succeeds()
    {
        var slug = _slugService.Slugify("a,d");
        Assert.Equal("a-d", slug);
    }

    [Theory]
    [InlineData("Smith, John B.")]
    [InlineData("Smith, John B...")]
    public void Remove_HyphansFromEnd_Succeeds(string input)
    {
        // Act
        var slug = _slugService.Slugify(input);
        Assert.Equal("smith-john-b", slug);
    }

    [Fact]
    public void Make_SureFunkycharactersAndHyphansOnlyReturnSingleHyphan_Succeeds()
    {
        var slug = _slugService.Slugify("«a»\"'-%-.d");
        Assert.Equal("a-d", slug);
    }

    [Fact]
    public void Convert_ToLowercase_Succeeds()
    {
        var slug = _slugService.Slugify("ABCDE");
        Assert.Equal("abcde", slug);
    }

    [Fact]
    public void Remove_Diacritics_Succeeds()
    {
        var slug = _slugService.Slugify("àçéïôù");
        Assert.Equal("aceiou", slug);
    }

    [Theory]
    [InlineData("джинсы_клеш", "джинсы_клеш")]
    [InlineData("צוות_אורצ_רד", "צוות_אורצ_רד")]
    [InlineData("调度模块允许后台任务调度", "调度模块允许后台任务调度")]
    [InlineData("فريق_الاورشارد", "فريق_الاورشارد")]
    [InlineData("不正なコンテナ", "不正なコンテナ")]
    public void Preserve_NonLatinCharacters_Succeeds(string input, string expected)
    {
        var slug = _slugService.Slugify(input);
        Assert.Equal(expected, slug);
    }

    [Fact]
    public void Transliterate_Requested_Succeeds()
    {
        var slug = _slugService.SlugifyAndTransliterate("Æneid");
        Assert.Equal("aeneid", slug);
    }
}
