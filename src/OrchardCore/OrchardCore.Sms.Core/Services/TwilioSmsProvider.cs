using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure;
using OrchardCore.Sms.Models;

namespace OrchardCore.Sms.Services;

public class TwilioSmsProvider : ISmsProvider
{
    public const string TechnicalName = "Twilio";

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
    };

    public LocalizedString Name => S["Twilio"];

    private readonly TwilioOptions _twilioOptions;
    private readonly ILogger<TwilioSmsProvider> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    protected readonly IStringLocalizer S;

    public TwilioSmsProvider(
        IOptions<TwilioOptions> twilioOptions,
        ILogger<TwilioSmsProvider> logger,
        IHttpClientFactory httpClientFactory,
        IStringLocalizer<TwilioSmsProvider> stringLocalizer)
    {
        _twilioOptions = twilioOptions.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        S = stringLocalizer;
    }

    public async Task<Result> SendAsync(SmsMessage message)
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
            var senderNumber = _twilioOptions.PhoneNumber;

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

            var client = GetHttpClient();
            var response = await client.PostAsync($"{_twilioOptions.AccountSID}/Messages.json", new FormUrlEncodedContent(data));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TwilioMessageResponse>(_jsonSerializerOptions);

                if (string.Equals(result.Status, "sent", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(result.Status, "queued", StringComparison.OrdinalIgnoreCase))
                {
                    return Result.Success();
                }

                _logger.LogError("Twilio service was unable to send SMS messages. Error, code: {ErrorCode}, message: {ErrorMessage}", result.ErrorCode, result.ErrorMessage);
            }

            return Result.Failed(S["The SMS message has not been sent."]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio service was unable to send SMS messages.");

            return Result.Failed(S["The SMS message has not been sent. Error: {0}", ex.Message]);
        }
    }

    private HttpClient GetHttpClient()
    {
        var token = $"{_twilioOptions.AccountSID}:{_twilioOptions.AuthToken}";
        var base64Token = Convert.ToBase64String(Encoding.ASCII.GetBytes(token));

        var client = _httpClientFactory.CreateClient(TechnicalName);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Token);

        return client;
    }
}
