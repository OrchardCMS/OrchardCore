using System.Threading.Tasks;

namespace OrchardCore.Email.Services;

public interface IEmailDeliveryService
{
    Task<IEmailResult> DeliverAsync(MailMessage message);
}
