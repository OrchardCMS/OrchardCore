using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.Email.Core.Services;

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

    public async Task<EmailResult> SendAsync(MailMessage message, string name = null)
    {
        var provider = await _providerResolver.GetAsync(name);

        if (provider is null)
        {
            _logger.LogError("Email settings must be configured before an Email message can be sent.");

            return EmailResult.FailedResult(S["Email settings must be configured before an Email message can be sent."]);
        }

        var validationContext = new MailMessageValidationContext(provider);

        await _emailServiceEvents.InvokeAsync((e) => e.ValidatingAsync(message, validationContext), _logger);

        await _emailServiceEvents.InvokeAsync((e) => e.ValidatedAsync(message, validationContext), _logger);

        if (validationContext.Errors.Count > 0)
        {
            await _emailServiceEvents.InvokeAsync((e) => e.FailedAsync(message), _logger);

            return EmailResult.FailedResult(validationContext.Errors);
        }

        await _emailServiceEvents.InvokeAsync((e) => e.SendingAsync(message), _logger);

        var result = await provider.SendAsync(message);

        if (result.Succeeded)
        {
            await _emailServiceEvents.InvokeAsync((e) => e.SentAsync(message), _logger);
        }
        else
        {
            await _emailServiceEvents.InvokeAsync((e) => e.FailedAsync(message), _logger);
        }

        return result;
    }
}
