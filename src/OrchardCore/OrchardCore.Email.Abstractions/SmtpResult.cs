using Microsoft.Extensions.Localization;

namespace OrchardCore.Email;

[Obsolete("This class should no longer be used. Instead use EmailResult.")]
public class SmtpResult : EmailResult
{
    /// <summary>
    /// Returns an <see cref="SmtpResult"/> indicating a successful Smtp operation.
    /// </summary>
    [Obsolete("This class should no longer be used. Instead use EmailResult.SuccessResult instead.")]
    public static readonly SmtpResult Success = new() { Succeeded = true };

    /// <summary>
    /// Creates an <see cref="EmailResult"/> indicating a failed Smtp operation, with a list of errors if applicable.
    /// </summary>
    /// <param name="errors">An optional array of <see cref="LocalizedString"/> which caused the operation to fail.</param>
    [Obsolete("This class should no longer be used. Instead use EmailResult.FailedResult instead.")]
    public static SmtpResult Failed(params LocalizedString[] errors)
        => new()
        {
            Succeeded = false,
            Errors = new Dictionary<string, LocalizedString[]>()
            {
                { string.Empty, errors }
            }
        };
}
