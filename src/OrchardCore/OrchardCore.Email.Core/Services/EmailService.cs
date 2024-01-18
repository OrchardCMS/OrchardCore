using System.Threading.Tasks;

namespace OrchardCore.Email.Services;

public class EmailService : IEmailService
{
    private readonly IEmailMessageValidator _emailMessageValidator;
    private readonly IEmailDeliveryService _emailDeliveryService;

    public EmailService(
        IEmailMessageValidator emailMessageValidator,
        IEmailDeliveryService emailDeliveryService)
    {
        _emailMessageValidator = emailMessageValidator;
        _emailDeliveryService = emailDeliveryService;
    }

    public async Task<EmailResult> SendAsync(MailMessage message)
    {
        _emailMessageValidator.Validate(message, out var errors);

        if (errors.Count > 0)
        {
            return EmailResult.Failed([.. errors]);
        }

        await _emailDeliveryService.DeliverAsync(message);

        return EmailResult.Success;
    }
}
