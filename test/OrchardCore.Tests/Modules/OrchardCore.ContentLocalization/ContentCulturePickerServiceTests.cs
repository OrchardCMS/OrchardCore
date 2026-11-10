using Microsoft.Net.Http.Headers;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentLocalization.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentLocalization;

public class ContentCulturePickerServiceTests
{
    [Theory]
    [InlineData("https", true)]
    [InlineData("http", false)]
    public void SetContentCulturePickerCookie_Scheme_SetsHttpOnlyAndSecureOnHttps(string scheme, bool expectedSecure)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = scheme;
        httpContext.Request.Host = new HostString("localhost");

        var service = new ContentCulturePickerService(
            autorouteEntries: null,
            localizationEntries: null,
            siteService: null,
            new HttpContextAccessor { HttpContext = httpContext },
            Options.Create(new CulturePickerOptions()));

        service.SetContentCulturePickerCookie("fr");

        // The first header deletes the previous cookie, the last one sets the new culture.
        var setCookie = SetCookieHeaderValue.Parse(httpContext.Response.Headers.SetCookie
            .Last(cookie => cookie.StartsWith($"{CookieRequestCultureProvider.DefaultCookieName}=", StringComparison.Ordinal)));

        Assert.Equal(CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("fr")), Uri.UnescapeDataString(setCookie.Value.Value));
        Assert.True(setCookie.HttpOnly);
        Assert.Equal(expectedSecure, setCookie.Secure);
    }
}
