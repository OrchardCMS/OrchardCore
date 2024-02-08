using System.Collections.Generic;
using System.Threading.Tasks;
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
            return EmailResult.FailedResult(S["Email settings must be configured before an Email message can be sent."]);
        }

        await _emailServiceEvents.InvokeAsync((e) => e.SendingAsync(message), _logger);

        var result = await provider.SendAsync(message);

        if (result.Succeeded)
        {
            await _emailServiceEvents.InvokeAsync((e) => e.SentAsync(), _logger);
        }

        return result;
    }
}
