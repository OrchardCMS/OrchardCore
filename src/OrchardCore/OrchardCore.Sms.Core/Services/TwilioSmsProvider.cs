using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;

namespace OrchardCore.Sms.Services;

public class TwilioSmsProvider : ISmsProvider
{
    public const string TechnicalName = "Twilio";

    public const string ProtectorName = "Twilio";

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
    };

    public LocalizedString Name => S["Twilio"];

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<TwilioSmsProvider> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    protected readonly IStringLocalizer S;

    public TwilioSmsProvider(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<TwilioSmsProvider> logger,
        IHttpClientFactory httpClientFactory,
        IStringLocalizer<TwilioSmsProvider> stringLocalizer)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        S = stringLocalizer;
    }

    public async Task<SmsResult> SendAsync(SmsMessage message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (String.IsNullOrEmpty(message.To))
        {
            throw new ArgumentException("A phone number is required in order to send a message.");
        }

        if (String.IsNullOrEmpty(message.Body))
        {
            throw new ArgumentException("A message body is required in order to send a message.");
        }

        try
        {
            var settings = await GetSettingsAsync();
            var data = new List<KeyValuePair<string, string>>
            {
                new ("From", settings.PhoneNumber),
                new ("To", message.To),
                new ("Body", message.Body),
            };

            var client = GetHttpClient(settings);
            var response = await client.PostAsync($"{settings.AccountSID}/Messages.json", new FormUrlEncodedContent(data));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TwilioMessageResponse>(_jsonSerializerOptions);

                if (String.Equals(result.Status, "sent", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(result.Status, "queued", StringComparison.OrdinalIgnoreCase))
                {
                    return SmsResult.Success;
                }

                _logger.LogError("Twilio service was unable to send SMS messages. Error, code: {errorCode}, message: {errorMessage}", result.ErrorCode, result.ErrorMessage);
            }

            return SmsResult.Failed(S["SMS message was not send."]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio service was unable to send SMS messages.");

            return SmsResult.Failed(S["SMS message was not send. Error: {0}", ex.Message]);
        }
    }

    private HttpClient GetHttpClient(TwilioSettings settings)
    {
        var client = _httpClientFactory.CreateClient(TechnicalName);

        var token = $"{settings.AccountSID}:{settings.AuthToken}";
        var base64Token = Convert.ToBase64String(Encoding.ASCII.GetBytes(token));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Token);

        return client;
    }

    private TwilioSettings _settings;

    private async Task<TwilioSettings> GetSettingsAsync()
    {
        if (_settings == null)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<TwilioSettings>();

            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);

            // It is important to create a new instance of `TwilioSettings` privately to hold the plain auth-token value.
            _settings = new TwilioSettings
            {
                PhoneNumber = settings.PhoneNumber,
                AccountSID = settings.AccountSID,
                AuthToken = protector.Unprotect(settings.AuthToken),
            };
        }

        return _settings;
    }
}
