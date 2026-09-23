using System.Net;
using OrchardCore.Workflows.Http.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Http;

public class HttpRequestTaskHttpClientTests
{
    public static TheoryData<string, bool> AddressCases => new()
    {
        { "8.8.8.8", true },
        { "10.0.0.1", false },
        { "100.64.0.1", false },
        { "127.0.0.1", false },
        { "169.254.169.254", false },
        { "172.16.0.1", false },
        { "192.168.0.1", false },
        { "198.51.100.1", false },
        { "2606:4700:4700::1111", true },
        { "::1", false },
        { "fc00::1", false },
        { "fd00:ec2::254", false },
        { "fe80::1", false },
        { "::ffff:169.254.169.254", false },
        { "64:ff9b::a9fe:a9fe", false },
        { "64:ff9b::808:808", true },
        { "2001:db8::1", false },
        { "3fff::1", false },
    };

    public static TheoryData<string, bool> UriCases => new()
    {
        { "https://example.com/path", true },
        { "http://example.com:8080/path", true },
        { "/relative/path", false },
        { "file:///etc/passwd", false },
        { "https://user:password@example.com/", false },
        { "not a URI", false },
    };

    [Theory]
    [MemberData(nameof(AddressCases))]
    public void IsPublicAddress_Address_ReturnsExpectedResult(string value, bool expected)
    {
        var result = HttpRequestTaskHttpClient.IsPublicAddress(IPAddress.Parse(value));

        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(UriCases))]
    public void TryCreateUri_Value_ReturnsExpectedResult(string value, bool expected)
    {
        var result = HttpRequestTaskHttpClient.TryCreateUri(value, out var uri);

        Assert.Equal(expected, result);
        Assert.Equal(expected, uri != null);
    }

    [Theory]
    [InlineData("http://127.1/")]
    [InlineData("http://2130706433/")]
    [InlineData("http://0x7f000001/")]
    [InlineData("http://0177.0.0.1/")]
    public void TryCreateUri_AlternativeLoopbackRepresentation_NormalizesToBlockedAddress(string value)
    {
        Assert.True(HttpRequestTaskHttpClient.TryCreateUri(value, out var uri));
        Assert.True(IPAddress.TryParse(uri.IdnHost, out var address));
        Assert.False(HttpRequestTaskHttpClient.IsPublicAddress(address));
    }

    [Fact]
    public async Task SendAsync_LoopbackDestination_ThrowsBlockedDestinationException()
    {
        using var handler = HttpRequestTaskHttpClient.CreateHandler();
        using var client = new HttpClient(handler);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.GetAsync("http://127.0.0.1:1", TestContext.Current.CancellationToken));

        Assert.True(HttpRequestTaskHttpClient.IsBlockedDestination(exception));
    }

    [Fact]
    public void CreateHandler_Defaults_DisableRedirectsAndProxy()
    {
        using var handler = HttpRequestTaskHttpClient.CreateHandler();

        Assert.False(handler.AllowAutoRedirect);
        Assert.False(handler.UseProxy);
    }
}
