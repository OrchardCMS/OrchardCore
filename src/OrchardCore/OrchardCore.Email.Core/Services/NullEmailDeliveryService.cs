using System.Threading.Tasks;

namespace OrchardCore.Email.Services;

public class NullEmailDeliveryService : IEmailDeliveryService
{
    public Task<EmailResult> DeliverAsync(MailMessage message) => Task.FromResult(EmailResult.Success);
}
