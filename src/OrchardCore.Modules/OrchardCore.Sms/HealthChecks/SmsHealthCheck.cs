using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;

namespace OrchardCore.Sms.HealthChecks;

internal sealed class SmsHealthCheck : IHealthCheck
{
    private readonly ISmsService _smsService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<TwilioOptions> _twilioOptions;

    public SmsHealthCheck(
        ISmsService smsService,
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<TwilioOptions> twilioOptions)
    {
        _smsService = smsService;
        _httpClientFactory = httpClientFactory;
        _twilioOptions = twilioOptions;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_smsService is null)
            {
                return HealthCheckResult.Unhealthy(description: $"The service '{nameof(ISmsService)}' isn't registered.");
            }

            var settings = _twilioOptions.CurrentValue;

            if (await ValidateTwilioCredentialsAsync(settings.AccountSID, settings.AuthToken))
            {
                return HealthCheckResult.Healthy();
            }
            else
            {
                return HealthCheckResult.Unhealthy(description: "The client is not connected to the Twilio service.");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(description: "Retrieving the status of the Twilio service failed.", ex);
        }
    }

    private async Task<bool> ValidateTwilioCredentialsAsync(string accountSid, string authToken)
    {
        using var client = _httpClientFactory.CreateClient(TwilioSmsProvider.TechnicalName);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}")));

        var response = await client.GetAsync($"https://api.twilio.com/2010-04-01/Accounts/{accountSid}.json", CancellationToken.None);

        return response.IsSuccessStatusCode;
    }
}
