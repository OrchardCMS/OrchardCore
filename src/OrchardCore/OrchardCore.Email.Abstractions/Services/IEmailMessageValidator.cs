using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace OrchardCore.Email.Services;

public interface IEmailMessageValidator
{
    bool IsValid(MailMessage message, out List<LocalizedString> errors);
}
