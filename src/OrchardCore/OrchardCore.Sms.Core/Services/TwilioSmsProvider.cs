using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Secrets;
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
    private readonly ISecretManager _secretManager;
    private readonly ILogger<TwilioSmsProvider> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    protected readonly IStringLocalizer S;

    public TwilioSmsProvider(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ISecretManager secretManager,
        ILogger<TwilioSmsProvider> logger,
        IHttpClientFactory httpClientFactory,
        IStringLocalizer<TwilioSmsProvider> stringLocalizer)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _secretManager = secretManager;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        S = stringLocalizer;
    }

    public async Task<SmsResult> SendAsync(SmsMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrEmpty(message.To))
        {
            throw new ArgumentException("A phone number is required in order to send a message.");
        }

        if (string.IsNullOrEmpty(message.Body))
        {
            throw new ArgumentException("A message body is required in order to send a message.");
        }

        try
        {
            var settings = await GetSettingsAsync();

            var senderNumber = settings.PhoneNumber;

            if (!string.IsNullOrEmpty(message.From))
            {
                senderNumber = message.From;
            }

            var data = new List<KeyValuePair<string, string>>
            {
                new ("From", senderNumber),
                new ("To", message.To),
                new ("Body", message.Body),
            };

            var client = GetHttpClient(settings);
            var response = await client.PostAsync($"{settings.AccountSID}/Messages.json", new FormUrlEncodedContent(data));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TwilioMessageResponse>(_jsonSerializerOptions);

                if (string.Equals(result.Status, "sent", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(result.Status, "queued", StringComparison.OrdinalIgnoreCase))
                {
                    return SmsResult.Success;
                }

                _logger.LogError("Twilio service was unable to send SMS messages. Error, code: {ErrorCode}, message: {ErrorMessage}", result.ErrorCode, result.ErrorMessage);
            }

            return SmsResult.Failed(S["The SMS message has not been sent."]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio service was unable to send SMS messages.");

            return SmsResult.Failed(S["The SMS message has not been sent. Error: {0}", ex.Message]);
        }
    }

    private HttpClient GetHttpClient(TwilioSettings settings)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var token = $"{settings.AccountSID}:{settings.AuthToken}";
#pragma warning restore CS0618 // Type or member is obsolete
        var base64Token = Convert.ToBase64String(Encoding.ASCII.GetBytes(token));

        var client = _httpClientFactory.CreateClient(TechnicalName);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Token);

        return client;
    }

    private TwilioSettings _settings;

    private async Task<TwilioSettings> GetSettingsAsync()
    {
        if (_settings == null)
        {
            var settings = await _siteService.GetSettingsAsync<TwilioSettings>();

            string authToken = null;

            // First try to load from secrets
            if (!string.IsNullOrWhiteSpace(settings.AuthTokenSecretName))
            {
                try
                {
                    var secret = await _secretManager.GetSecretAsync<TextSecret>(settings.AuthTokenSecretName);

                    if (secret != null && !string.IsNullOrWhiteSpace(secret.Text))
                    {
                        authToken = secret.Text;

                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("Twilio auth token loaded from secret '{SecretName}'.", settings.AuthTokenSecretName);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Twilio auth token secret '{SecretName}' was not found or is empty.", settings.AuthTokenSecretName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load Twilio auth token from secret '{SecretName}'.", settings.AuthTokenSecretName);
                }
            }

            // Fall back to legacy encrypted auth token
#pragma warning disable CS0618 // Type or member is obsolete
            if (authToken == null && !string.IsNullOrEmpty(settings.AuthToken))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(ProtectorName);
                    authToken = protector.Unprotect(settings.AuthToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "The Twilio auth token could not be decrypted. It may have been encrypted using a different key.");
                }
            }
#pragma warning restore CS0618 // Type or member is obsolete

            // It is important to create a new instance of `TwilioSettings` privately to hold the plain auth-token value.
            _settings = new TwilioSettings
            {
                PhoneNumber = settings.PhoneNumber,
                AccountSID = settings.AccountSID,
#pragma warning disable CS0618 // Type or member is obsolete
                AuthToken = authToken,
#pragma warning restore CS0618 // Type or member is obsolete
            };
        }

        return _settings;
    }
}
