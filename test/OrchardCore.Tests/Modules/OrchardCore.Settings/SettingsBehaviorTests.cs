using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Settings;

public sealed class SettingsBehaviorTests
{
    [Theory]
    [MemberData(nameof(AssertBaseUrlData))]
    public void SiteSettings_BaseUrl_ShouldBeValid(string baseUrl, bool isValid) =>
        AssertBaseUrl(baseUrl, isValid, (site, value) => site.BaseUrl = value, site => site.BaseUrl);

    [Theory]
    [MemberData(nameof(AssertBaseUrlData))]
    public void SiteSettings_CdnBaseUrl_ShouldBeValid(string baseUrl, bool isValid) =>
        AssertBaseUrl(baseUrl, isValid, (site, value) => site.CdnBaseUrl = value, site => site.CdnBaseUrl);

    public static IEnumerable<object[]> AssertBaseUrlData => new object[][]
    {
        [null, true],
        ["https://example.com/foo/", true],
        ["example.com/foo/", false],
        ["x\"><script>alert('cross-user');document.title='XSS-CROSS-USER'</script><link href=\"", false],
    };
    
    private static void AssertBaseUrl(string baseUrl, bool isValid, Action<ISite, string> set, Func<ISite, string> get)
    {
        // Arrange
        var settings = new SiteSettings();
        Exception exception = null;

        // Act
        try
        {
            set(settings, baseUrl);
        }
        catch (Exception e)
        {
            exception = e;
        }

        // Assert
        if (isValid)
        {
            Assert.Null(exception);
            Assert.Equal(baseUrl, get(settings));
        }
        else
        {
            Assert.NotNull(exception);
            Assert.Null(get(settings));
        }
    }
}
