namespace OrchardCore.DataPipelines.Sftp;

/// <summary>
/// Reads and compares the SHA-256 fingerprints of SSH host keys, in the format the <c>ssh</c> command shows, such as
/// <c>SHA256:ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og</c>. The <c>SHA256:</c> prefix and the Base64 padding are
/// optional.
/// </summary>
public static class DataPipelineHostKeyFingerprint
{
    private const string Prefix = "SHA256:";
    private const int HashLength = 32;

    /// <summary>
    /// Tells whether a text is a SHA-256 fingerprint.
    /// </summary>
    /// <param name="fingerprint">The text.</param>
    /// <returns><see langword="true"/> when it is a SHA-256 fingerprint.</returns>
    public static bool IsValid(string fingerprint)
        => Normalize(fingerprint) is not null;

    /// <summary>
    /// Normalizes a SHA-256 fingerprint to unpadded Base64, without the <c>SHA256:</c> prefix.
    /// </summary>
    /// <param name="fingerprint">The fingerprint.</param>
    /// <returns>The normalized fingerprint, or <see langword="null"/> when the text is not a SHA-256 fingerprint.</returns>
    public static string Normalize(string fingerprint)
    {
        var value = fingerprint?.Trim();

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = value[Prefix.Length..].Trim();
        }

        value = value.TrimEnd('=');

        // Unpadded Base64 of 32 bytes is 43 characters long.
        if (value.Length != 43)
        {
            return null;
        }

        Span<byte> hash = stackalloc byte[HashLength + 1];

        return Convert.TryFromBase64String(value + "=", hash, out var length) && length == HashLength ? value : null;
    }

    /// <summary>
    /// Tells whether the fingerprint of the key a server presented is the expected one.
    /// </summary>
    /// <param name="expected">The expected fingerprint, in any accepted format.</param>
    /// <param name="actual">The fingerprint of the key the server presented, in any accepted format.</param>
    /// <returns><see langword="true"/> when they are the same.</returns>
    public static bool Matches(string expected, string actual)
    {
        var normalizedExpected = Normalize(expected);

        return normalizedExpected is not null && string.Equals(normalizedExpected, Normalize(actual), StringComparison.Ordinal);
    }
}
