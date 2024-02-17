using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email.Core.Services;

public class EmailMessageValidator : EmailServiceEventsBase
{
    private readonly IEmailAddressValidator _emailAddressValidator;

    protected readonly IStringLocalizer S;

    public EmailMessageValidator(IEmailAddressValidator emailAddressValidator,
        IStringLocalizer<EmailMessageValidator> stringLocalizer)
    {
        _emailAddressValidator = emailAddressValidator;
        S = stringLocalizer;
    }

    public override Task ValidatingAsync(MailMessage message, MailMessageValidationContext context)
    {
        context.Errors.AddRange(message.GetSender()
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the sender: '{0}'.", address]));

        var recipients = message.GetRecipients();

        context.Errors.AddRange(recipients.To
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]));

        context.Errors.AddRange(recipients.Cc
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]));

        context.Errors.AddRange(recipients.Bcc
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]));

        context.Errors.AddRange(message.GetReplyTo()
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]));

        if (recipients.To.Count == 0 && recipients.Cc.Count == 0 && recipients.Bcc.Count == 0)
        {
            context.Errors.Add(S["The mail message should have at least one of these headers: To, Cc or Bcc."]);
        }

        return Task.CompletedTask;
    }
}
