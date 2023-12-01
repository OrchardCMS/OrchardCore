using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email.Services;

public class NullEmailService : EmailServiceBase<EmailSettings>
{
    public NullEmailService(
        IOptions<EmailSettings> options,
        ILogger<NullEmailService> logger,
        IStringLocalizer<NullEmailService> stringLocalizer,
        IEmailAddressValidator emailAddressValidator) : base(options, logger, stringLocalizer, emailAddressValidator)
    {
    }

    public override Task<EmailResult> SendAsync(MailMessage message) => Task.FromResult(EmailResult.Success);
}
