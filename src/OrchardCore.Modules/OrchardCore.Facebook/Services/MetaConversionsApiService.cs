using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Facebook.Models;
using OrchardCore.Facebook.Settings;
using OrchardCore.Infrastructure;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Services;

public sealed class MetaConversionsApiService : IMetaConversionsApiService
{
    // https://developers.facebook.com/docs/graph-api/changelog - bump periodically.
    private const string GraphApiVersion = "v21.0";

    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _httpClient;
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;
    private readonly IStringLocalizer S;

    public MetaConversionsApiService(
        HttpClient httpClient,
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<MetaConversionsApiService> logger,
        IStringLocalizer<MetaConversionsApiService> stringLocalizer)
    {
        _httpClient = httpClient;
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
        S = stringLocalizer;
    }

    public async Task<Result> SendEventAsync(MetaConversionEvent conversionEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversionEvent);

        if (string.IsNullOrWhiteSpace(conversionEvent.EventName))
        {
            return Result.Failed(S["The event name is required."]);
        }

        var settings = await _siteService.GetSettingsAsync<FacebookPixelSettings>();

        if (string.IsNullOrWhiteSpace(settings.PixelId))
        {
            return Result.Failed(S["The Meta Pixel is not configured. Set the Pixel ID under Settings > Meta Pixel."]);
        }

        if (string.IsNullOrWhiteSpace(settings.ConversionsApiAccessToken))
        {
            return Result.Failed(S["The Meta Conversions API access token is not configured. Set it under Settings > Meta Pixel."]);
        }

        string accessToken;
        try
        {
            var protector = _dataProtectionProvider.CreateProtector(FacebookConstants.ConversionsApiProtectorName);
            accessToken = protector.Unprotect(settings.ConversionsApiAccessToken);
        }
        catch (CryptographicException ex)
        {
            _logger.LogError(ex, "The Meta Conversions API access token could not be decrypted. It may have been encrypted using a different key.");

            return Result.Failed(S["The Meta Conversions API access token could not be decrypted. Re-enter it under Settings > Meta Pixel."]);
        }

        var payload = new MetaConversionsApiRequest
        {
            Data =
            [
                new MetaConversionsApiEvent
                {
                    EventName = conversionEvent.EventName,
                    EventTime = (conversionEvent.EventTime ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds(),
                    EventSourceUrl = conversionEvent.EventSourceUrl,
                    ActionSource = ToActionSourceValue(conversionEvent.ActionSource),
                    EventId = conversionEvent.EventId,
                },
            ],
            TestEventCode = string.IsNullOrWhiteSpace(settings.ConversionsApiTestEventCode)
                ? null
                : settings.ConversionsApiTestEventCode,
        };

        var requestUri = $"{GraphApiVersion}/{settings.PixelId}/events?access_token={Uri.EscapeDataString(accessToken)}";

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(requestUri, payload, s_serializerOptions, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogError(
                "The Meta Conversions API request failed with status {StatusCode}: {Body}",
                (int)response.StatusCode,
                body);

            return Result.Failed(S["The Meta Conversions API request failed with status {0}.", (int)response.StatusCode]);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "An error occurred connecting to the Meta Conversions API.");

            return Result.Failed(S["An error occurred connecting to the Meta Conversions API."]);
        }
    }

    private static string ToActionSourceValue(MetaActionSource actionSource)
        => actionSource switch
        {
            MetaActionSource.Website => "website",
            MetaActionSource.Email => "email",
            MetaActionSource.App => "app",
            MetaActionSource.PhoneCall => "phone_call",
            MetaActionSource.Chat => "chat",
            MetaActionSource.PhysicalStore => "physical_store",
            MetaActionSource.SystemGenerated => "system_generated",
            _ => "other",
        };

    private sealed class MetaConversionsApiRequest
    {
        [JsonPropertyName("data")]
        public MetaConversionsApiEvent[] Data { get; set; }

        [JsonPropertyName("test_event_code")]
        public string TestEventCode { get; set; }
    }

    private sealed class MetaConversionsApiEvent
    {
        [JsonPropertyName("event_name")]
        public string EventName { get; set; }

        [JsonPropertyName("event_time")]
        public long EventTime { get; set; }

        [JsonPropertyName("event_source_url")]
        public string EventSourceUrl { get; set; }

        [JsonPropertyName("action_source")]
        public string ActionSource { get; set; }

        [JsonPropertyName("event_id")]
        public string EventId { get; set; }
    }
}
