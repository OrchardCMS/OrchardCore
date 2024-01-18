using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace OrchardCore.Email.Services;

public interface IEmailMessageValidator
{
    void Validate(MailMessage message, out List<LocalizedString> errors);
}
