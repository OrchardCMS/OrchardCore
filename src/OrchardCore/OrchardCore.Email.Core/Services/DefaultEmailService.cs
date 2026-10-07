using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Infrastructure;
using OrchardCore.Modules;

namespace OrchardCore.Email.Services;

public class DefaultEmailService : IEmailService
{
    private readonly IEmailProviderResolver _providerResolver;
    private readonly IEnumerable<IEmailServiceEvents> _emailServiceEvents;
    private readonly ILogger _logger;

    protected readonly IStringLocalizer S;

    public DefaultEmailService(
        IEmailProviderResolver providerResolver,
        IEnumerable<IEmailServiceEvents> emailServiceEvents,
        ILogger<DefaultEmailService> logger,
        IStringLocalizer<DefaultEmailService> stringLocalizer)
    {
        _providerResolver = providerResolver;
        _emailServiceEvents = emailServiceEvents;
        _logger = logger;
        S = stringLocalizer;
    }

    /// <summary>
    /// Sends the specified email message by using the selected provider or the default provider.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="providerName">The technical name of the email provider to use. When null or empty, the default provider is used.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Result"/> describing whether the email was sent successfully.</returns>
    public async Task<Result> SendAsync(MailMessage message, string providerName = null, CancellationToken cancellationToken = default)
    {
        var provider = await _providerResolver.GetAsync(providerName);

        if (provider is null)
        {
            _logger.LogError("Email settings must be configured before an Email message can be sent.");

            return Result.Failed(S["Email settings must be configured before an Email message can be sent."]);
        }

        var validationContext = new MailMessageValidationContext(provider);

        await _emailServiceEvents.InvokeAsync((e, token) => e.ValidatingAsync(message, validationContext, token), cancellationToken, _logger);

        await _emailServiceEvents.InvokeAsync((e, token) => e.ValidatedAsync(message, validationContext, token), cancellationToken, _logger);

        if (validationContext.Errors.Count > 0)
        {
            await _emailServiceEvents.InvokeAsync((e, token) => e.FailedAsync(message, token), cancellationToken, _logger);

            var resultErrors = validationContext.Errors.SelectMany(kvp => kvp.Value.Select(error => new ResultError
            {
                Key = kvp.Key,
                Message = error,
            }));

            return Result.Failed(resultErrors);
        }

        await _emailServiceEvents.InvokeAsync((e, token) => e.SendingAsync(message, token), cancellationToken, _logger);

        var result = await provider.SendAsync(message, cancellationToken);

        if (result.Succeeded)
        {
            await _emailServiceEvents.InvokeAsync((e, token) => e.SentAsync(message, token), cancellationToken, _logger);
        }
        else
        {
            await _emailServiceEvents.InvokeAsync((e, token) => e.FailedAsync(message, token), cancellationToken, _logger);
        }

        return result;
    }
}
