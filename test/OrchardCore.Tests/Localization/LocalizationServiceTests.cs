using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class LocalizationService
{
    [Fact]
    public void GetCultures_ShouldContainsChineseCultures()
    {
        // Act
        var cultures = ILocalizationService.GetCultures();

        // Assert
        Assert.Contains(cultures, c => c.Name == "zh-CN");
        Assert.Contains(cultures, c => c.Name == "zh-TW");
    }
}
