using Microsoft.Extensions.Compliance.Redaction;

namespace OrchardCore.Users.AuditTrail.Services;

/// <summary>
/// Similar to <see cref="ErasingRedactor"/>, but it returns <see langword="null"/> when redacting a string, so it can
/// be used to indicate that the value should be removed instead of just emptied.
/// </summary>
public class RemoveRedactor : Redactor
{
    public override int Redact(ReadOnlySpan<char> source, Span<char> destination) => 0;

    public override int GetRedactedLength(ReadOnlySpan<char> input) => 0;

    /// <inheritdoc/>
    public override string Redact(string source) => null;
}
