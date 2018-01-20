using System.Threading.Tasks;
using OrchardCore.Email.Models;

namespace OrchardCore.Email.Services
{
    public interface ISmtpService
    {
        /// <summary>
        /// A new instance of this service will be instantiated with
        /// the <paramref name="settings"/>.
        /// </summary>
        /// <returns>The settings.</returns>
        ISmtpService WithSettings(SmtpSettings settings);
        Task<SmtpResult> SendAsync (EmailMessage emailMessage);
    }
}