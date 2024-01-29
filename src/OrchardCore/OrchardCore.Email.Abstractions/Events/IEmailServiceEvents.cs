using System.Threading.Tasks;

namespace OrchardCore.Email.Events;

public interface IEmailServiceEvents
{
    Task OnMessageSendingAsync(MailMessage message);

    Task OnMessageSentAsync();
}
