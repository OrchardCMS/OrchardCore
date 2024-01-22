using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Sms;

public class SmsResult
{
    /// <summary>
    /// Returns an <see cref="SmsResult"/> indicating a successful SMS operation.
    /// </summary>
    public static SmsResult Success { get; } = new() { Succeeded = true };

    /// <summary>
    /// An <see cref="IEnumerable{LocalizedString}"/> containing an errors that occurred during the SMS operation.
    /// </summary>
    public IEnumerable<LocalizedString> Errors { get; protected set; }

    /// <summary>
    /// Get or sets the response text from the SMS server.
    /// </summary>
    public string Response { get; set; }

    /// <summary>
    /// Whether the operation succeeded or not.
    /// </summary>
    public bool Succeeded { get; protected set; }

    /// <summary>
    /// Creates an <see cref="SmsResult"/> indicating a failed SMS operation, with a list of errors if applicable.
    /// </summary>
    /// <param name="errors">An optional array of <see cref="LocalizedString"/> which caused the operation to fail.</param>
    public static SmsResult Failed(params LocalizedString[] errors)
        => new()
        {
            Succeeded = false,
            Errors = errors
        };
}
