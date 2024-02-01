using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Email.Events;
using OrchardCore.Modules;

namespace OrchardCore.Email.Services;

public class EmailService : IEmailService
{
    private readonly IEmailMessageValidator _emailMessageValidator;
    private readonly IEmailDeliveryServiceResolver _emailDeliveryServiceResolver;
    private readonly IEnumerable<IEmailServiceEvents> _emailServiceEvents;
    private readonly ILogger _logger;

    public EmailService(
        IEmailMessageValidator emailMessageValidator,
        IEmailDeliveryServiceResolver emailDeliveryServiceResolver,
        IEnumerable<IEmailServiceEvents> emailServiceEvents,
        ILogger<EmailService> logger)
    {
        _emailMessageValidator = emailMessageValidator;
        _emailDeliveryServiceResolver = emailDeliveryServiceResolver;
        _emailServiceEvents = emailServiceEvents;
        _logger = logger;
    }

    public async Task<IEmailResult> SendAsync(MailMessage message, string deliveryServiceName = null)
    {
        await _emailServiceEvents.InvokeAsync((e, message) => e.OnMessageSendingAsync(message), message, _logger);

        if (!_emailMessageValidator.IsValid(message, out var errors))
        {
            return EmailResult.Failed([.. errors]);
        }

        var emailDeliveryService = _emailDeliveryServiceResolver.Resolve(deliveryServiceName);

        var result = await emailDeliveryService.DeliverAsync(message);

        await _emailServiceEvents.InvokeAsync((e) => e.OnMessageSentAsync(), _logger);

        return result;
    }
}
