using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email.Services;

public class EmailMessageValidator : IEmailMessageValidator
{
    private readonly IEmailAddressValidator _emailAddressValidator;
    private readonly EmailSettings _emailSettings;

    protected readonly IStringLocalizer S;

    public EmailMessageValidator(IEmailAddressValidator emailAddressValidator,
        IOptions<EmailSettings> options,
        IStringLocalizer<EmailMessageValidator> stringLocalizer)
    {
        _emailAddressValidator = emailAddressValidator;
        _emailSettings = options.Value;
        S = stringLocalizer;
    }

    public bool IsValid(MailMessage message, out List<LocalizedString> errors)
    {
        errors = [];
        var senderAddress = string.IsNullOrWhiteSpace(message.Sender)
            ? _emailSettings.DefaultSender
            : message.Sender;

        if (!string.IsNullOrEmpty(senderAddress))
        {
            if (!_emailAddressValidator.Validate(senderAddress))
            {
                errors.Add(S["Invalid email address for the sender: '{0}'.", senderAddress]);
            }
        }

        errors.AddRange(message.GetSender()
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the sender: '{0}'.", address]));

        var recipients = message.GetRecipients();

        errors.AddRange(recipients.To
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]));

        errors.AddRange(recipients.Cc
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]));

        errors.AddRange(recipients.Bcc
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]));

        errors.AddRange(message.GetReplyTo()
            .Where(address => !_emailAddressValidator.Validate(address))
            .Select(address => S["Invalid email address for the recipient: '{0}'.", address]));

        if (recipients.To.Count == 0 && recipients.Cc.Count == 0 && recipients.Bcc.Count == 0)
        {
            errors.Add(S["The mail message should have at least one of these headers: To, Cc or Bcc."]);
        }

        return errors.Count == 0;
    }
}
