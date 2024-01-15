using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Smtp.Services;

/// <summary>
/// Represents the result of sending an email.
/// </summary>
public class SmtpEmailResult : EmailResult
{
    public SmtpEmailResult(bool succeeded)
    {
        Succeeded = succeeded;
    }

    public SmtpEmailResult(IEnumerable<LocalizedString> errors)
    {
        Errors = errors;
        Succeeded = false;
    }

    /// <summary>
    /// Get or sets the response text from the SMTP server.
    /// </summary>
    public string Response { get; set; }
}
