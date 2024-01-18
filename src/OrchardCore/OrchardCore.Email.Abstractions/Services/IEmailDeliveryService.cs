using System.Threading.Tasks;

namespace OrchardCore.Email.Services;

public interface IEmailDeliveryService
{
    Task<EmailResult> DeliverAsync(MailMessage message);
}
