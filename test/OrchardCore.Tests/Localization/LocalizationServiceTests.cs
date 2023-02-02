using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class LocalizationService
{
    [Fact]
    public void GetCulturesAndAliases_ShouldContainsChineseCultures()
    {
        // Act
        var cultures = ILocalizationService.GetCulturesAndAliases();

        // Assert
        Assert.Contains(cultures, c => c.Name == "zh-CN");
        Assert.Contains(cultures, c => c.Name == "zh-TW");
    }
}
