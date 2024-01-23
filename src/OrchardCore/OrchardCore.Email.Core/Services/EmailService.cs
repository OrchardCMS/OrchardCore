using System.Threading.Tasks;

namespace OrchardCore.Email.Services;

public class EmailService : IEmailService
{
    private readonly IEmailMessageValidator _emailMessageValidator;
    private readonly IEmailDeliveryServiceResolver _emailDeliveryServiceResolver;

    public EmailService(
        IEmailMessageValidator emailMessageValidator,
        IEmailDeliveryServiceResolver emailDeliveryServiceResolver)
    {
        _emailMessageValidator = emailMessageValidator;
        _emailDeliveryServiceResolver = emailDeliveryServiceResolver;
    }

    public async Task<EmailResult> SendAsync(MailMessage message, string deliveryMethodName = null)
    {
        _emailMessageValidator.Validate(message, out var errors);

        if (errors.Count > 0)
        {
            return EmailResult.Failed([.. errors]);
        }

        var emailDeliveryService = _emailDeliveryServiceResolver.Resolve(deliveryMethodName);

        return await emailDeliveryService.DeliverAsync(message);
    }
}
