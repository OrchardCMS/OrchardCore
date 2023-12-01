using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email.Services;

/// <summary>
/// Represents an email service that do nothing.
/// </summary>
/// <param name="options">The <see cref="IOptions{AzureEmailSettings}"/>.</param>
/// <param name="logger">The <see cref="ILogger{AzureEmailService}"/>.</param>
/// <param name="stringLocalizer">The <see cref="IStringLocalizer{AzureEmailService}"/>.</param>
/// <param name="emailAddressValidator">The <see cref="IEmailAddressValidator"/>.</param>
public class NullEmailService(
    IOptions<EmailSettings> options,
    ILogger<NullEmailService> logger,
    IStringLocalizer<NullEmailService> stringLocalizer,
    IEmailAddressValidator emailAddressValidator) : EmailServiceBase<EmailSettings>(options, logger, stringLocalizer, emailAddressValidator)
{
    /// <inheritdoc/>
    public override Task<EmailResult> SendAsync(MailMessage message) => Task.FromResult(EmailResult.Success);
}
