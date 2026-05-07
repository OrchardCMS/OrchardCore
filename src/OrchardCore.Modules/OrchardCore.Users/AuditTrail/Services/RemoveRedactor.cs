using Microsoft.Extensions.Compliance.Redaction;

namespace OrchardCore.Users.AuditTrail.Services;

public class RemoveRedactor : Redactor
{
    public const string RemoveProperty = "!!Remove Property";

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        var value = new ReadOnlySpan<char>(RemoveProperty.ToCharArray());

        // Avoid buffer overflows by returning nothing if there is no space. 
        return !value.TryCopyTo(destination) ? 0 : value.Length;
    }

    public override int GetRedactedLength(ReadOnlySpan<char> input) => RemoveProperty.Length;

    /// <inheritdoc/>
    public override string Redact(string source) => RemoveProperty;
}
