using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Tests.Sitemaps;

public class SitemapBuilderContextTests
{
    [Theory]
    [InlineData("https://example.com", "https://example.com/guides")]
    [InlineData("https://example.com/", "https://example.com/guides")]
    public async Task CustomPathUrl_BaseUrlFromSiteSettings_HasNoDuplicateSlash(string baseUrl, string expectedLoc)
    {
        // SiteSettings returns the base URL with a trailing slash, as the sitemap controller passes it.
        var siteSettings = new SiteSettings { BaseUrl = baseUrl };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(x => x.Content("~/guides")).Returns("/guides");

        XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var context = new SitemapBuilderContext
        {
            HostPrefix = siteSettings.BaseUrl,
            UrlHelper = urlHelper.Object,
            Response = new SitemapResponse { ResponseElement = new XElement(sitemapNamespace + "urlset") },
        };

        await new CustomPathSitemapSourceBuilder().BuildSourceAsync(new CustomPathSitemapSource { Path = "guides" }, context);

        Assert.Equal(expectedLoc, context.Response.ResponseElement.Descendants(sitemapNamespace + "loc").Single().Value);
    }
}
