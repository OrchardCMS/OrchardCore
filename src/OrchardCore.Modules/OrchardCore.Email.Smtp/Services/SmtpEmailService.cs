using OrchardCore.Email.Services;

namespace OrchardCore.Email.Smtp.Services;

public class SmtpEmailService : EmailService
{
    public SmtpEmailService(
        IEmailMessageValidator emailMessageValidator, SmtpEmailDeliveryService smtpEmailDeliveryService)
        : base(emailMessageValidator, smtpEmailDeliveryService)
    {
    }
}
