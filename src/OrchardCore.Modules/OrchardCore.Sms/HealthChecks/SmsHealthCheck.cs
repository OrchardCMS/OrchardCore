using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;

namespace OrchardCore.Sms.HealthChecks;

public class SmsHealthCheck : IHealthCheck
{
    private readonly ISmsService _smsService;
    private readonly ISiteService _siteService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public SmsHealthCheck(
        ISmsService smsService,
        ISiteService siteService,
        IHttpClientFactory httpClientFactory,
        IDataProtectionProvider dataProtectionProvider)
    {
        _smsService = smsService;
        _siteService = siteService;
        _httpClientFactory = httpClientFactory;
        _dataProtectionProvider = dataProtectionProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_smsService is null)
            {
                return HealthCheckResult.Unhealthy(description: $"The service '{nameof(ISmsService)}' isn't registered.");
            }

            var settings = await _siteService.GetSettingsAsync<TwilioSettings>();

            var dataProtector = _dataProtectionProvider.CreateProtector(TwilioSmsProvider.ProtectorName);

            if (await ValidateTwilioCredentialsAsync(settings.AccountSID, dataProtector.Unprotect(settings.AuthToken)))
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
        var client = _httpClientFactory.CreateClient(TwilioSmsProvider.TechnicalName);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}")));

        var response = await client.GetAsync($"https://api.twilio.com/2010-04-01/Accounts/{accountSid}.json", CancellationToken.None);

        return response.IsSuccessStatusCode;
    }
}
