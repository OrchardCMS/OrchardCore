using System.Net.Mail;
using System.Threading.Tasks;

namespace OrchardCore.Email
{
    public interface ISmtpService
    {
        /// <summary>
        /// Sends the specified message to an SMTP server for delivery.
        /// </summary>
        Task<SmtpResult> SendAsync(MailMessage message);
    }
}