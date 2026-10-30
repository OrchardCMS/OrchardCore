using Microsoft.AspNetCore.Http;
using OrchardCore.Notifications;

namespace OrchardCore.Tests.Modules.OrchardCore.Notifications;

public class NotificationUrlHelperTests
{
    [Theory]
    [InlineData("~/blog/post-1")]
    [InlineData("~/")]
    [InlineData("/admin/comments?id=1#c1")]
    [InlineData("https://example.com/orders/1")]
    [InlineData("http://example.com")]
    public void IsValid_LocalOrHttpUrl_ReturnsTrue(string url)
    {
        Assert.True(NotificationUrlHelper.IsValid(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html;base64,PHNjcmlwdD4=")]
    [InlineData("//evil.example.com")]
    [InlineData("/\\evil.example.com")]
    [InlineData("~//evil.example.com")]
    [InlineData("ftp://example.com/file")]
    [InlineData("relative/path")]
    [InlineData("/path\nwith-newline")]
    public void IsValid_UnsafeOrRelativeUrl_ReturnsFalse(string url)
    {
        Assert.False(NotificationUrlHelper.IsValid(url));
    }

    [Theory]
    [InlineData("~/blog", "", "/blog")]
    [InlineData("~/blog?page=2", "/tenant1", "/tenant1/blog?page=2")]
    [InlineData("/blog", "/tenant1", "/blog")]
    [InlineData("https://example.com/a", "/tenant1", "https://example.com/a")]
    public void Resolve_ValidUrl_ReturnsBrowserUrl(string url, string pathBase, string expected)
    {
        Assert.Equal(expected, NotificationUrlHelper.Resolve(url, new PathString(pathBase)));
    }

    [Fact]
    public void Resolve_InvalidUrl_ReturnsNull()
    {
        Assert.Null(NotificationUrlHelper.Resolve("javascript:alert(1)", PathString.Empty));
    }
}
