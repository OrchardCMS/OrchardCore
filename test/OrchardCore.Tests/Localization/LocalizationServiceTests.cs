using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class LocalizationService
{
    [Fact]
    public void GetAllCulturesAndAliases_ShouldContainsChineseCultures()
    {
        // Act
        var cultures = ILocalizationService.GetAllCulturesAndAliases();

        // Assert
        Assert.Single(cultures, c => c.Name == "zh-CN");
        Assert.Single(cultures, c => c.Name == "zh-TW");
    }
}
