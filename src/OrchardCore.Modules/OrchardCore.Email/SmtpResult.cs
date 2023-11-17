using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email
{
    /// <summary>
    /// Represents the result of sending an email.
    /// </summary>
    public class SmtpResult : EmailResult
    {
        public SmtpResult(bool succeeded)
        {
            Succeeded = succeeded;
        }

        public SmtpResult(IEnumerable<LocalizedString> errors)
        {
            Errors = errors;
            Succeeded = false;
        }

        /// <summary>
        /// Get or sets the response text from the SMTP server.
        /// </summary>
        public string Response { get; set; }
    }
}
