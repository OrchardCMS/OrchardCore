using OrchardCore.Localization;
using OrchardCore.Localization.Services;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Localization;

public class LocalizationServiceTest
{
    [Fact]
    public void GetAllCulturesAndAliases_ShouldContainsChineseCultures()
    {
        // Arrange
        var localizationService = new LocalizationService(Mock.Of<ISiteService>());

        // Act
        var cultures = localizationService.GetAllCulturesAndAliases();

        // Assert
        Assert.Single(cultures, c => c.Name == "zh-CN");
        Assert.Single(cultures, c => c.Name == "zh-TW");
    }
}
