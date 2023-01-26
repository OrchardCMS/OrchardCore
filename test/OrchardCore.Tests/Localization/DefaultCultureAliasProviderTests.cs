using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class DefaultCultureAliasProviderTests
{
    [Theory]
    [InlineData("zh-Hans-CN", "zh-CN", true)]
    [InlineData("zh-Hant-TW", "zh-TW", true)]
    [InlineData("zh-SG", null, false)]
    public void ShouldReturnAliasFromCulture(string culture, string expectedCulture, bool expectedAliasFound)
    {
        // Arrange
        var cultureOptions = Options.Create(new CultureOptions());
        var cultureAliasProvider = new DefaultCultureAliasProvider(cultureOptions);

        // Act
        var isAliasFound = cultureAliasProvider.TryGetCulture(culture, out var cultureAlias);

        // Assert
        Assert.Equal(expectedAliasFound, isAliasFound);
        Assert.Equal(expectedCulture, cultureAlias?.Name);
    }

    [Theory]
    [InlineData("zh-Hans-CN", "zh-CN", true)]
    [InlineData("zh-Hant-TW", "zh-TW", false)]
    public void ShouldRespect_OrchardCoreRequestLocalizationOptions_IgnoreSystemCulture(string culture, string expectedCulture, bool ignoreSystemCulture)
    {
        // Arrange
        var cultureOptions = Options.Create(new CultureOptions
        {
            IgnoreSystemSettings = ignoreSystemCulture
        });
        var cultureAliasProvider = new DefaultCultureAliasProvider(cultureOptions);

        // Act
        var isAliasFound = cultureAliasProvider.TryGetCulture(culture, out var cultureAlias);

        // Assert
        Assert.True(isAliasFound);
        Assert.Equal(expectedCulture, cultureAlias.Name);
        Assert.Equal(ignoreSystemCulture, !cultureAlias.UseUserOverride);
    }
}
