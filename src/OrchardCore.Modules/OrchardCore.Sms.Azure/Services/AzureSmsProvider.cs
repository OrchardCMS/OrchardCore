using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Communication.Sms;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.Services;

public sealed class AzureSmsProvider : ISmsProvider
{
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<AzureSmsProvider> _logger;

    private AzureSmsSettings _settings;

    internal readonly IStringLocalizer S;

    public const string TechnicalName = "Azure";

    public const string ProtectorName = "Azure";

    public LocalizedString Name => S["Azure"];

    public AzureSmsProvider(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<AzureSmsProvider> logger,
        IStringLocalizer<AzureSmsProvider> stringLocalizer)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
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
            var data = new List<KeyValuePair<string, string>>
            {
                new ("From", settings.PhoneNumber),
                new ("To", message.To),
                new ("Body", message.Body),
            };

            var client = new SmsClient(settings.ConnectionString);
            var response = await client.SendAsync(settings.PhoneNumber, message.To, message.Body);

            if (response.Value.Successful)
            {
                return SmsResult.Success;
            }
            return SmsResult.Failed(S["SMS message was not send."]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure service was unable to send SMS messages.");

            return SmsResult.Failed(S["SMS message was not send. Error: {0}", ex.Message]);
        }
    }

    private async Task<AzureSmsSettings> GetSettingsAsync()
    {
        if (_settings == null)
        {
            var settings = await _siteService.GetSiteSettingsAsync<AzureSmsSettings>();

            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);

            _settings = new AzureSmsSettings
            {
                ConnectionString = settings.ConnectionString == null ? null : protector.Unprotect(settings.ConnectionString),
                PhoneNumber = settings.PhoneNumber
            };
        }

        return _settings;
    }
}
