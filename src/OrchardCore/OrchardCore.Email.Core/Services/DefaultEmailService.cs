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

    private IEmailProvider _provider;

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
        _provider ??= await _providerResolver.GetAsync();

        if (_provider is null)
        {
            return EmailResult.FailedResult(S["Email settings must be configured before an Email message can be sent."]);
        }

        await _emailServiceEvents.InvokeAsync((e) => e.SendingAsync(message), _logger);

        var result = await _provider.SendAsync(message);

        if (result.Succeeded)
        {
            await _emailServiceEvents.InvokeAsync((e) => e.SentAsync(), _logger);
        }

        return result;
    }
}
