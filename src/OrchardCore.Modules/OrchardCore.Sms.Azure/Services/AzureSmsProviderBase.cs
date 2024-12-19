using Azure.Communication.Sms;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.Services;

public abstract class AzureSmsProviderBase : ISmsProvider
{
    private readonly AzureSmsOptions _providerOptions;
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly ILogger _logger;

    private SmsClient _smsClient;

    protected readonly IStringLocalizer S;

    public AzureSmsProviderBase(
        AzureSmsOptions options,
        IPhoneFormatValidator phoneFormatValidator,
        ILogger logger,
        IStringLocalizer stringLocalizer)
    {
        _providerOptions = options;
        _phoneFormatValidator = phoneFormatValidator;
        _logger = logger;
        S = stringLocalizer;
    }

    public abstract LocalizedString Name { get; }

    public virtual async Task<SmsResult> SendAsync(SmsMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_providerOptions.IsEnabled)
        {
            return SmsResult.Failed(S["The Azure Communication Provider is disabled."]);
        }

        _logger.LogDebug("Attempting to send an SMS message using Azure Communication service to {Recipient}.", message.To);

        if (string.IsNullOrWhiteSpace(message.To))
        {
            return SmsResult.Failed(S["A phone number is required for the recipient.", message.To]);
        }

        if (!_phoneFormatValidator.IsValid(message.To))
        {
            return SmsResult.Failed(S["Invalid phone number format for the recipient: '{0}'.", message.To]);
        }

        if (string.IsNullOrEmpty(message.Body))
        {
            return SmsResult.Failed(S["The message body is required.", message.To]);
        }

        try
        {
            _smsClient ??= new SmsClient(_providerOptions.ConnectionString);

            var response = await _smsClient.SendAsync(_providerOptions.PhoneNumber, message.To, message.Body);

            if (response.Value.Successful)
            {
                return SmsResult.Success;
            }

            return SmsResult.Failed(S["SMS message was not send."]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending an SMS using the Azure SMS Provider.");

            return SmsResult.Failed(S["An error occurred while sending an SMS."]);
        }
    }
}
