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
        var invalidSender = message.GetSender()
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the sender: '{0}'.", address]);

        AddError(context, invalidSender);

        var recipients = message.GetRecipients();

        var invalidTo = recipients.To
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]);

        AddError(context, invalidTo);

        var invalidCc = recipients.Cc
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]);

        AddError(context, invalidCc);

        var invalidBcc = recipients.Bcc
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]);

        AddError(context, invalidBcc);

        var invalidReplayTo = message.GetReplyTo()
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]);

        AddError(context, invalidReplayTo);

        if (recipients.To.Count == 0 && recipients.Cc.Count == 0 && recipients.Bcc.Count == 0)
        {
            AddError(context, [S["The mail message should have at least one of these headers: To, Cc or Bcc."]]);
        }

        return Task.CompletedTask;
    }

    private static void AddError(MailMessageValidationContext context, IEnumerable<LocalizedString> errorMessages)
    {
        if (!errorMessages.Any())
        {
            return;
        }

        context.Errors.AddRange(errorMessages);
    }
}
