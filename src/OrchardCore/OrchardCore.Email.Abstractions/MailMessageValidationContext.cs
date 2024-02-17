using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email;

public class MailMessageValidationContext
{
    public List<LocalizedString> Errors { get; } = [];
}
