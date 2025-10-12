using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class DefaultCultureAliasProviderTests
{
    [Fact]
    public void GetAllCulturesAndAliases_ShouldContainsChineseCultures()
    {
        // Arrange
        var provider = new DefaultCultureAliasProvider();

        // Act
        var cultures = provider.GetCultureAliases();

        // Assert
        Assert.Single(cultures, c => c.Name == "zh-CN");
        Assert.Single(cultures, c => c.Name == "zh-TW");
    }
}
