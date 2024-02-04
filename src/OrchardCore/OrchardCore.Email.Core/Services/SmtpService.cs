using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Email.Services;

/// <summary>
/// Represents a SMTP service that allows to send emails.
/// </summary>
public class SmtpService : ISmtpService
{
    private readonly IEmailService _emailProviderResolver;

    public SmtpService(IEmailService emailProviderResolver)
    {
        _emailProviderResolver = emailProviderResolver;
    }

    public async Task<SmtpResult> SendAsync(MailMessage message)
    {
        var provider = await _emailProviderResolver.GetAsync(SmtpEmailProvider.TechnicalName);

        var result = await provider.SendAsync(message);

        if (result.Succeeded)
        {
            return SmtpResult.Success;
        }

        return SmtpResult.Failed(result.Errors.ToArray());
    }
}
