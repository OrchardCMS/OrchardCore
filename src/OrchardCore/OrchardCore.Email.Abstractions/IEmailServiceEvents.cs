using System.Threading.Tasks;

namespace OrchardCore.Email;

public interface IEmailServiceEvents
{
    /// <summary>
    /// This event is triggered before sending the email via the email provider.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SendingAsync(MailMessage message);

    /// <summary>
    /// This event is triggered after the email was successfully sent.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SentAsync(MailMessage message);

    /// <summary>
    /// This event is triggered if the email fails to send.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task FailedAsync(MailMessage message);
}
