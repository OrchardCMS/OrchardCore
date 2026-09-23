using System.Net;
using Microsoft.Extensions.Options;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Http.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Http;

public class HttpRequestDestinationValidatorTests
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
    public void IsAllowed_Address_ReturnsExpectedResult(string value, bool expected)
    {
        var validator = CreateValidator();

        var result = validator.IsAllowed("example.com", IPAddress.Parse(value));

        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(UriCases))]
    public void TryCreateUri_Value_ReturnsExpectedResult(string value, bool expected)
    {
        var result = HttpRequestDestinationValidator.TryCreateUri(value, out var uri, out _);

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
        var validator = CreateValidator();

        Assert.True(HttpRequestDestinationValidator.TryCreateUri(value, out var uri, out _));
        Assert.True(IPAddress.TryParse(uri.IdnHost, out var address));
        Assert.False(validator.IsAllowed(uri.IdnHost, address));
    }

    [Fact]
    public void IsAllowed_ConfiguredHost_AllowsPrivateAddress()
    {
        var validator = CreateValidator(new HttpRequestTaskOptions
        {
            AllowedHosts = ["internal.example.com"],
        });

        Assert.True(validator.IsAllowed("INTERNAL.EXAMPLE.COM.", IPAddress.Parse("10.0.0.1")));
    }

    [Fact]
    public void IsAllowed_ConfiguredNetwork_AllowsPrivateAddress()
    {
        var validator = CreateValidator(new HttpRequestTaskOptions
        {
            AllowedIpNetworks = ["10.20.0.0/16"],
        });

        Assert.True(validator.IsAllowed("internal.example.com", IPAddress.Parse("10.20.30.40")));
        Assert.False(validator.IsAllowed("internal.example.com", IPAddress.Parse("10.21.30.40")));
    }

    [Fact]
    public void IsAllowed_StrictMode_BlocksUnconfiguredPublicAddress()
    {
        var validator = CreateValidator(new HttpRequestTaskOptions
        {
            AllowOnlyConfiguredDestinations = true,
        });

        Assert.False(validator.IsAllowed("example.com", IPAddress.Parse("8.8.8.8")));
    }

    [Fact]
    public void CreateValidator_InvalidNetwork_ThrowsInvalidOperationException()
    {
        var options = Options.Create(new HttpRequestTaskOptions
        {
            AllowedIpNetworks = ["not-a-network"],
        });

        Assert.Throws<InvalidOperationException>(() => new HttpRequestDestinationValidator(options));
    }

    [Fact]
    public async Task SendAsync_LoopbackDestination_ThrowsBlockedDestinationException()
    {
        using var handler = HttpRequestTaskHttpClient.CreateHandler(CreateValidator());
        using var client = new HttpClient(handler);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.GetAsync("http://127.0.0.1:1", TestContext.Current.CancellationToken));

        Assert.True(ContainsBlockedDestinationException(exception));
    }

    [Fact]
    public void CreateHandler_Defaults_DisableStatefulAndBypassFeatures()
    {
        using var handler = HttpRequestTaskHttpClient.CreateHandler(CreateValidator());

        Assert.False(handler.AllowAutoRedirect);
        Assert.False(handler.UseCookies);
        Assert.False(handler.UseProxy);
    }

    private static HttpRequestDestinationValidator CreateValidator(HttpRequestTaskOptions options = null)
        => new(Options.Create(options ?? new HttpRequestTaskOptions()));

    private static bool ContainsBlockedDestinationException(Exception exception)
    {
        while (exception != null)
        {
            if (exception is HttpRequestDestinationNotAllowedException)
            {
                return true;
            }

            exception = exception.InnerException;
        }

        return false;
    }
}
