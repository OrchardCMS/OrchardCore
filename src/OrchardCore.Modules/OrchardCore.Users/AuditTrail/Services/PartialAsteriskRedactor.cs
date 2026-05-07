using Microsoft.Extensions.Compliance.Redaction;

namespace OrchardCore.Users.AuditTrail.Services;

public class PartialAsteriskRedactor : Redactor
{
    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        if (destination.Length == 0)
        {
            return 0;
        }

        destination[0] = source[0];

        for (var i = 1; i < source.Length - 1 && i < destination.Length; i++)
        {
            destination[i] = '*';
        }

        if (destination.Length >= source.Length)
        {
            destination[source.Length - 1] = source[^1];
        }

        return Math.Min(source.Length, destination.Length);
    }

    public override int GetRedactedLength(ReadOnlySpan<char> input) => input.Length;

    public override string Redact(string source) =>
        (source?.Length ?? 0) <= 2 
            ? source
            : $"{source![0]}{new string('*', source.Length - 2)}{source[^1]}";
}
