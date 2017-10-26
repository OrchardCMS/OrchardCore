using System.Threading.Tasks;
using OrchardCore.Email.Models;

namespace OrchardCore.Email.Services
{
    public interface ISmtpService
    {
        Task SendAsync(EmailMessage emailMessage);
    }
}
