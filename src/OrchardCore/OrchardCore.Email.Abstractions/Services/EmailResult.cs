using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email.Services
{
    /// <summary>
    /// Represents the result of sending an email.
    /// </summary>
    public class EmailResult : IEmailResult
    {
        /// <summary>
        /// Returns an <see cref="IEmailResult"/> indicating a successful email operation.
        /// </summary>
        public static IEmailResult Success { get; } = new EmailResult() { Succeeded = true };

        /// <summary>
        /// An <see cref="IEnumerable{LocalizedString}"/> containing an errors that occurred during the email operation.
        /// </summary>
        public IEnumerable<LocalizedString> Errors { get; protected set; }

        /// <summary>
        /// Whether the operation succeeded or not.
        /// </summary>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// Creates an <see cref="EmailResult"/> indicating a failed email operation, with a list of errors if applicable.
        /// </summary>
        /// <param name="errors">An optional array of <see cref="LocalizedString"/> which caused the operation to fail.</param>
        public static IEmailResult Failed(params LocalizedString[] errors) => new EmailResult() { Succeeded = false, Errors = errors };
    }
}
