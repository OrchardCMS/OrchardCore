using System.Globalization;
using Microsoft.Extensions.Compliance.Redaction;

namespace OrchardCore.Users.AuditTrail.Services;

/// <summary>
/// Substitutes all but the first and last text element with asterisks. Ideal for redacting emails or names.
/// </summary>
public class PartialAsteriskRedactor : Redactor
{
    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        var requiredLength = GetRedactedLength(source);
        if (destination.Length < requiredLength)
        {
            throw new ArgumentException("The destination is too short for the redacted value.", nameof(destination));
        }

        if (source.IsEmpty)
        {
            return 0;
        }

        var firstLength = StringInfo.GetNextTextElementLength(source);
        source[..firstLength].CopyTo(destination);
        var written = firstLength;
        var position = firstLength;

        while (position < source.Length)
        {
            var length = StringInfo.GetNextTextElementLength(source[position..]);
            if (position + length == source.Length)
            {
                source[position..].CopyTo(destination[written..]);
                written += length;
            }
            else
            {
                destination[written++] = '*';
            }

            position += length;
        }

        return written;
    }

    public override int GetRedactedLength(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
        {
            return 0;
        }

        var firstLength = StringInfo.GetNextTextElementLength(input);
        var length = firstLength;
        var position = firstLength;

        while (position < input.Length)
        {
            var nextLength = StringInfo.GetNextTextElementLength(input[position..]);
            position += nextLength;
            length += position == input.Length ? nextLength : 1;
        }

        return length;
    }

    public override string Redact(string source) => source is null ? null : base.Redact(source);
}
