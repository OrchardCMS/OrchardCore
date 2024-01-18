using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email.Services;

public class EmailMessageValidator : IEmailMessageValidator
{
    private readonly IEmailAddressValidator _emailAddressValidator;
    private readonly EmailSettings _emailSettings;
    private readonly IStringLocalizer S;

    public EmailMessageValidator(IEmailAddressValidator emailAddressValidator,
        IOptions<EmailSettings> options,
        IStringLocalizer<EmailMessageValidator> stringLocalizer)
    {
        _emailAddressValidator = emailAddressValidator;
        _emailSettings = options.Value;
        S = stringLocalizer;
    }

    public void Validate(MailMessage message, out List<LocalizedString> errors)
    {
        errors = [];
        var submitterAddress = string.IsNullOrWhiteSpace(message.Sender)
            ? _emailSettings.DefaultSender
            : message.Sender;

        if (!string.IsNullOrEmpty(submitterAddress))
        {
            if (!_emailAddressValidator.Validate(submitterAddress))
            {
                errors.Add(S["Invalid email address: '{0}'", submitterAddress]);
            }
        }

        errors.AddRange(message.GetSender()
            .Where(a => !_emailAddressValidator.Validate(a))
            .Select(a => S["Invalid email address: '{0}'", a]));

        var recipients = message.GetRecipients();

        errors.AddRange(recipients.To
            .Where(r => !_emailAddressValidator.Validate(r))
            .Select(r => S["Invalid email address: '{0}'", r]));

        errors.AddRange(recipients.Cc
            .Where(r => !_emailAddressValidator.Validate(r))
            .Select(r => S["Invalid email address: '{0}'", r]));

        errors.AddRange(recipients.Bcc
            .Where(r => !_emailAddressValidator.Validate(r))
            .Select(r => S["Invalid email address: '{0}'", r]));

        errors.AddRange(message.GetReplyTo()
            .Where(r => !_emailAddressValidator.Validate(r))
            .Select(r => S["Invalid email address: '{0}'", r]));

        if (recipients.To.Count == 0 && recipients.Cc.Count == 0 && recipients.Bcc.Count == 0)
        {
            errors.Add(S["The mail message should have at least one of these headers: To, Cc or Bcc."]);
        }
    }
}
