using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrchardCore.Entities;
using OrchardCore.Facebook;
using OrchardCore.Facebook.Models;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Facebook;

public class MetaConversionsApiServiceTests
{
    private static readonly EphemeralDataProtectionProvider s_dataProtectionProvider = new();

    [Fact]
    public async Task SendEventAsyncFailsWhenEventNameIsMissing()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"), new FacebookPixelSettings
        {
            PixelId = "123",
            ConversionsApiAccessToken = Protect("token"),
        });

        var result = await service.SendEventAsync(new MetaConversionEvent(), TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SendEventAsyncFailsWhenPixelIdIsMissing()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"), new FacebookPixelSettings
        {
            ConversionsApiAccessToken = Protect("token"),
        });

        var result = await service.SendEventAsync(new MetaConversionEvent { EventName = "Lead" }, TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SendEventAsyncFailsWhenAccessTokenIsMissing()
    {
        var service = CreateService(new FakeHttpMessageHandler(HttpStatusCode.OK, "{}"), new FacebookPixelSettings
        {
            PixelId = "123",
        });

        var result = await service.SendEventAsync(new MetaConversionEvent { EventName = "Lead" }, TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SendEventAsyncSendsExpectedPayloadAndSucceedsOnOk()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{\"events_received\":1}");

        var service = CreateService(handler, new FacebookPixelSettings
        {
            PixelId = "123456",
            ConversionsApiAccessToken = Protect("my-access-token"),
            ConversionsApiTestEventCode = "TEST12345",
        });

        var result = await service.SendEventAsync(new MetaConversionEvent
        {
            EventName = "Purchase",
            EventSourceUrl = "https://example.com/checkout",
            ActionSource = MetaActionSource.Website,
            EventId = "order-42",
        }, TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.NotNull(handler.LastRequest);

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.StartsWith("v21.0/123456/events", handler.LastRequest.RequestUri.PathAndQuery.TrimStart('/'), StringComparison.Ordinal);
        Assert.Contains("access_token=my-access-token", handler.LastRequest.RequestUri.Query, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(handler.LastRequestBody);
        var root = document.RootElement;

        Assert.Equal("TEST12345", root.GetProperty("test_event_code").GetString());

        var evt = root.GetProperty("data")[0];
        Assert.Equal("Purchase", evt.GetProperty("event_name").GetString());
        Assert.Equal("https://example.com/checkout", evt.GetProperty("event_source_url").GetString());
        Assert.Equal("website", evt.GetProperty("action_source").GetString());
        Assert.Equal("order-42", evt.GetProperty("event_id").GetString());
        Assert.True(evt.GetProperty("event_time").GetInt64() > 0);
    }

    [Fact]
    public async Task SendEventAsyncOmitsTestEventCodeWhenNotConfigured()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");

        var service = CreateService(handler, new FacebookPixelSettings
        {
            PixelId = "123456",
            ConversionsApiAccessToken = Protect("token"),
        });

        var result = await service.SendEventAsync(new MetaConversionEvent { EventName = "Lead" }, TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);

        using var document = JsonDocument.Parse(handler.LastRequestBody);

        Assert.False(document.RootElement.TryGetProperty("test_event_code", out _));
    }

    [Fact]
    public async Task SendEventAsyncFailsWhenGraphApiReturnsAnError()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, "{\"error\":{\"message\":\"Invalid parameter\"}}");

        var service = CreateService(handler, new FacebookPixelSettings
        {
            PixelId = "123456",
            ConversionsApiAccessToken = Protect("token"),
        });

        var result = await service.SendEventAsync(new MetaConversionEvent { EventName = "Lead" }, TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
    }

    private static string Protect(string value)
        => s_dataProtectionProvider.CreateProtector(FacebookConstants.ConversionsApiProtectorName).Protect(value);

    private static MetaConversionsApiService CreateService(HttpMessageHandler handler, FacebookPixelSettings settings)
    {
        var siteService = new Mock<ISiteService>();
        var site = new SiteSettings();
        site.Put(settings);

        siteService.Setup(x => x.GetSiteSettingsAsync()).ReturnsAsync(site);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.facebook.com/"),
        };

        return new MetaConversionsApiService(
            httpClient,
            siteService.Object,
            s_dataProtectionProvider,
            NullLogger<MetaConversionsApiService>.Instance,
            new Mock<IStringLocalizer<MetaConversionsApiService>>().Object.OrFallback());
    }
}

file static class StringLocalizerExtensions
{
    // Moq doesn't implement the indexer by default; provide a trivial pass-through localizer.
    public static IStringLocalizer<T> OrFallback<T>(this IStringLocalizer<T> _)
        => new FallbackStringLocalizer<T>();
}

file sealed class FallbackStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name);

    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}

internal sealed class FakeHttpMessageHandler(HttpStatusCode statusCode, string responseBody) : HttpMessageHandler
{
    public HttpRequestMessage LastRequest { get; private set; }

    public string LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody),
        };
    }
}
