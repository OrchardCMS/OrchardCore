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
        var cultureAliasInfo = CultureInfo.GetCultureInfo(cultureAlias);

        // Act
        var isAliasFound = cultureAliasProvider.TryGetCulture(cultureAliasInfo, out var culture);

        // Assert
        Assert.Equal(expectedAliasFound, isAliasFound);
        Assert.Equal(expectedCulture, culture?.Name);
    }

    [Theory]
    [InlineData("zh-CN", "zh-Hans-CN", true)]
    [InlineData("zh-TW", "zh-Hant-TW", false)]
    public void ShouldRespect_OrchardCoreRequestLocalizationOptions_IgnoreSystemCulture(string cultureAlias, string expectedCulture, bool ignoreSystemCulture)
    {
        // Arrange
        var cultureOptions = Options.Create(new CultureOptions
        {
            IgnoreSystemSettings = ignoreSystemCulture
        });
        var cultureAliasProvider = new DefaultCultureAliasProvider(cultureOptions);
        var cultureAliasInfo = CultureInfo.GetCultureInfo(cultureAlias);

        // Act
        var isAliasFound = cultureAliasProvider.TryGetCulture(cultureAliasInfo, out var culture);

        // Assert
        Assert.True(isAliasFound);
        Assert.Equal(expectedCulture, culture.Name);
        Assert.Equal(ignoreSystemCulture, !culture.UseUserOverride);
    }
}
