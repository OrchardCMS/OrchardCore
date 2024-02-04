using System.Threading.Tasks;

namespace OrchardCore.Email;

public interface IEmailServiceEvents
{
    Task SendingAsync(MailMessage message);

    Task SentAsync();
}
