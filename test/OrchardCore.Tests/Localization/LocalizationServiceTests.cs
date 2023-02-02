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
        Assert.Contains(cultures, c => c.Name == "zh-CN");
        Assert.Contains(cultures, c => c.Name == "zh-TW");
    }
}
