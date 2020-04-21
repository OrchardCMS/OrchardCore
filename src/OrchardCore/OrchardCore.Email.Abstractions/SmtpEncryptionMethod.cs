namespace OrchardCore.Email
{
    /// <summary>
    /// Represents an enumeration for mail encryption methods.
    /// </summary>
    public enum SmtpEncryptionMethod
    {
        None = 0,
        SSLTLS = 1,
        STARTTLS = 2
    }
}
