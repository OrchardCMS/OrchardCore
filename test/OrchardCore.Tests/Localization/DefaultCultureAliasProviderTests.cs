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
        var cultureOptions = Options.Create(new CultureOptions());
        var cultureAliasProvider = new DefaultCultureAliasProvider(cultureOptions);

        // Act
        var isAliasFound = cultureAliasProvider.TryGetCulture(cultureAlias, out var culture);

        // Assert
        Assert.Equal(expectedAliasFound, isAliasFound);
        Assert.Equal(expectedCulture, culture?.Name);
    }
}
