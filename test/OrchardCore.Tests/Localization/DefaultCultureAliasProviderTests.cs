using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class DefaultCultureAliasProviderTests
{
    [Theory]
    [InlineData("zh-CN", "zh-Hans-CN", true)]
    [InlineData("zh-TW", "zh-Hant-TW", true)]
    [InlineData("zh-SG", null, false)]
    public void ShouldReturnCultureFromAlias(string cultureAlias, string expectedCulture, bool expectedAliasFound)
    {
        // Arrange
        var cultureAliasProvider = new DefaultCultureAliasProvider();
        var cultureAliasInfo = CultureInfo.GetCultureInfo(cultureAlias);

        // Act
        var isAliasFound = cultureAliasProvider.TryGetCulture(cultureAliasInfo, out var culture);

        // Assert
        Assert.Equal(expectedAliasFound, isAliasFound);
        Assert.Equal(expectedCulture, culture?.Name);
    }
}
