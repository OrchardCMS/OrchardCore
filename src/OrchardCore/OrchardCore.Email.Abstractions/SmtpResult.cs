using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email
{
    /// <summary>
    /// Represents the result of sending an email.
    /// </summary>
    public class SmtpResult
    {
        /// <summary>
        /// Returns an <see cref="SmtpResult"/> indicating a successful Smtp operation.
        /// </summary>
        public static SmtpResult Success { get; } = new SmtpResult { Succeeded = true };

        /// <summary>
        /// An <see cref="IEnumerable{LocalizedString}"/> containing an errors that occurred during the Smtp operation.
        /// </summary>
        public IEnumerable<LocalizedString> Errors { get; protected set; }

        /// <summary>
        /// Get or sets the response text from the SMTP server.
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// Whether the operation succeeded or not.
        /// </summary>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// Creates an <see cref="SmtpResult"/> indicating a failed Smtp operation, with a list of errors if applicable.
        /// </summary>
        /// <param name="errors">An optional array of <see cref="LocalizedString"/> which caused the operation to fail.</param>
        public static SmtpResult Failed(params LocalizedString[] errors) => new() { Succeeded = false, Errors = errors };
    }
}
