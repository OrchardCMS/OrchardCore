using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Indexing.Models;

/// <summary>Reports rejected index-profile values before they are persisted.</summary>
public sealed class IndexProfileValidationException : ValidationException
{
    /// <summary>Creates an exception containing the index handlers' validation failures.</summary>
    public IndexProfileValidationException(IReadOnlyList<ValidationResult> errors)
        : base(string.Join(" ", errors.Select(error => error.ErrorMessage)))
    {
        Errors = errors;
    }

    /// <summary>Gets the validation failures and their associated member names.</summary>
    public IReadOnlyList<ValidationResult> Errors { get; }
}
