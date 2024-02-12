using System.Threading.Tasks;

namespace OrchardCore.Email;

public interface IEmailServiceEvents
{
    /// <summary>
    /// This event is fired before sending the email via the email provider.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SendingAsync(MailMessage message);

    /// <summary>
    /// This event is fired after the email was successfully sent.
    /// </summary>
    /// <returns></returns>
    Task SentAsync();
}
