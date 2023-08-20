using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace OrchardCore.Sms.Services;

public class TwilioSmsProvider : ISmsProvider
{
    public const string TechnicalName = "Twilio";

    public const string ProtectorName = "Twilio";

    public LocalizedString Name => S["Twilio"];

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger<TwilioSmsProvider> _logger;
    protected readonly IStringLocalizer S;
    private TwilioSettings _settings;

    public TwilioSmsProvider(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<TwilioSmsProvider> logger,
        IStringLocalizer<TwilioSmsProvider> stringLocalizer)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
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

            TwilioClient.Init(settings.AccountSID, settings.AuthToken);

            var response = await MessageResource.CreateAsync(
                to: new PhoneNumber(message.To),
                from: new PhoneNumber(settings.PhoneNumber),
                body: message.Body
            );

            if (response.Status == MessageResource.StatusEnum.Sent
                || response.Status == MessageResource.StatusEnum.Queued)
            {
                return SmsResult.Success;
            }

            _logger.LogError("Twilio service is unable to send SMS messages. Error: {errorMessage}", response.ErrorMessage);

            return SmsResult.Failed(S["SMS message was not send."]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio service is unable to send SMS messages.");

            return SmsResult.Failed(S["SMS message was not send. Error: {0}", ex.Message]);
        }
    }

    private async Task<TwilioSettings> GetSettingsAsync()
    {
        if (_settings == null)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<TwilioSettings>();

            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);

            // It is important here to create a new instance of `TwilioSettings` privately to hold the plain token value.
            _settings = new TwilioSettings()
            {
                PhoneNumber = settings.PhoneNumber,
                AccountSID = settings.AccountSID,
                AuthToken = protector.Unprotect(settings.AuthToken),
            };
        }

        return _settings;
    }
}
