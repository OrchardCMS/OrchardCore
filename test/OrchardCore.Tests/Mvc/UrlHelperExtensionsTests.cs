using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Moq;
using OrchardCore.Mvc.Core.Utilities;

namespace OrchardCore.Tests.Mvc;

public class UrlHelperExtensionsTests
{
    [Fact]
    public void ToAbsoluteUrlReturnsInputUnchangedWhenItIsAlreadyAbsolute()
    {
        // A CDN base URL (see IMediaFileStore.MapPathToPublicUrl()) produces an already-absolute
        // URL. ToAbsoluteUrl() must not prefix the site's own base URL onto it (see #12835).
        var urlHelper = CreateUrlHelper("www.mydomain.com");

        var result = urlHelper.Object.ToAbsoluteUrl("https://cdn.mydomain.com/media/image.jpg");

        Assert.Equal("https://cdn.mydomain.com/media/image.jpg", result);
    }

    [Theory]
    [InlineData("http://cdn.mydomain.com/media/image.jpg")]
    [InlineData("https://cdn.mydomain.com/media/image.jpg")]
    public void ToAbsoluteUrlPreservesSchemeOfAlreadyAbsoluteUrls(string absoluteUrl)
    {
        var urlHelper = CreateUrlHelper("www.mydomain.com");

        var result = urlHelper.Object.ToAbsoluteUrl(absoluteUrl);

        Assert.Equal(absoluteUrl, result);
    }

    [Fact]
    public void ToAbsoluteUrlPrefixesBaseUrlForRelativePaths()
    {
        var urlHelper = CreateUrlHelper("www.mydomain.com");
        urlHelper.Setup(x => x.Content("/media/image.jpg")).Returns("/media/image.jpg");

        var baseUrl = urlHelper.Object.GetBaseUrl();
        var content = urlHelper.Object.Content("/media/image.jpg");
        var result = urlHelper.Object.ToAbsoluteUrl("/media/image.jpg");

        Assert.Equal("https://www.mydomain.com", baseUrl);
        Assert.Equal("/media/image.jpg", content);
        Assert.Equal("https://www.mydomain.com/media/image.jpg", result);
    }

    [Fact]
    public void ToAbsoluteUrlDoesNotMistakeARootedPathForAnAbsoluteUrl()
    {
        // Guard against treating an ordinary rooted local path (no scheme) as already
        // absolute, or local/relative media paths would never get the site base URL prefixed.
        var urlHelper = CreateUrlHelper("www.mydomain.com");
        urlHelper.Setup(x => x.Content("/media/image.jpg")).Returns("/media/image.jpg");

        var result = urlHelper.Object.ToAbsoluteUrl("/media/image.jpg");

        Assert.Equal("https://www.mydomain.com/media/image.jpg", result);
    }

    [Theory]
    [InlineData("HTTP://cdn.mydomain.com/media/image.jpg")]
    [InlineData("HTTPS://cdn.mydomain.com/media/image.jpg")]
    [InlineData("Https://cdn.mydomain.com/media/image.jpg")]
    public void ToAbsoluteUrlSchemeCheckIsCaseInsensitive(string absoluteUrl)
    {
        var urlHelper = CreateUrlHelper("www.mydomain.com");

        var result = urlHelper.Object.ToAbsoluteUrl(absoluteUrl);

        Assert.Equal(absoluteUrl, result);
    }

    private static Mock<IUrlHelper> CreateUrlHelper(string host)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString(host);

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.SetupGet(x => x.ActionContext).Returns(actionContext);

        return urlHelper;
    }
}
