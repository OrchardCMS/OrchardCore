using Microsoft.Extensions.Localization;
using OrchardCore.Infrastructure;

namespace OrchardCore.Sms.Services;

public class SmsService : ISmsService
{
    private readonly ISmsProviderResolver _smsProviderResolver;
    private ISmsProvider _provider;

    protected readonly IStringLocalizer S;

    public SmsService(
        ISmsProviderResolver smsProviderResolver,
        IStringLocalizer<SmsService> stringLocalizer)
    {
        _smsProviderResolver = smsProviderResolver;
        S = stringLocalizer;
    }

    /// <summary>
    /// Sends the specified SMS message by using the configured default SMS provider.
    /// </summary>
    /// <param name="message">The SMS message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Result"/> describing whether the SMS was sent successfully.</returns>
    public async Task<Result> SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        _provider ??= await _smsProviderResolver.GetAsync();

        if (_provider is null)
        {
            return Result.Failed(S["SMS settings must be configured before an SMS message can be sent."]);
        }

        return await _provider.SendAsync(message, cancellationToken);
    }
}
