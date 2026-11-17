using OrchardCore.Localization;

namespace OrchardCore.Tests.Abstractions;

public class StringUriExtensionsTests
{
    [Theory]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    public void ToSnakeCase_DoesNotDependOnCurrentCulture(string culture)
    {
        // Under tr-TR, "I".ToLower() is "ı" (dotless i).
        using var cultureScope = CultureScope.Create(culture);

        Assert.Equal("is_enabled", "IsEnabled".ToSnakeCase());
        Assert.Equal("error_code", "ErrorCode".ToSnakeCase());
    }
}
