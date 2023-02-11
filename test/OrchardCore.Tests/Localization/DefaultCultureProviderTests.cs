using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class DefaultCultureProviderTests
{
    [Fact]
    public void ShouldContainsChineseCultures_WithDefaultCultureAliasProvider()
    {
        // Arrange
        var cultureAliasProviders = new List<ICultureAliasProvider>
        {
            new DefaultCultureAliasProvider()
        };
        var cultureProvider = new DefaultCultureProvider(cultureAliasProviders);

        // Act
        var cultures = cultureProvider.GetAllCulturesAndAliases();

        // Assert
        Assert.Single(cultures, c => c.Name == "zh-CN");
        Assert.Single(cultures, c => c.Name == "zh-TW");
    }
}
