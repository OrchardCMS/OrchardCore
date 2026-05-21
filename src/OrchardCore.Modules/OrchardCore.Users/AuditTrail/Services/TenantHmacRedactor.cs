using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using System.Security.Cryptography;
using System.Text;

namespace OrchardCore.Users.AuditTrail.Services;

public class TenantHmacRedactor : Redactor
{
    private readonly HmacRedactor _hmacRedactor;

    public TenantHmacRedactor(ShellSettings shellSettings)
    {
        _hmacRedactor = new HmacRedactor(new OptionsWrapper<HmacRedactorOptions>(new HmacRedactorOptions
        {
            // Ensures distinct hashes for different tenants.
            Key = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(shellSettings.TenantId))),
            // Only used for correlation.
            KeyId = shellSettings.TenantId.GetHashCode(),
        }));
    }

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination) =>
        _hmacRedactor.Redact(source, destination);

    public override int GetRedactedLength(ReadOnlySpan<char> input) =>
        _hmacRedactor.GetRedactedLength(input);
}
