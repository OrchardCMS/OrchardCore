namespace OrchardCore.Email
{
    // TODO: The enum has be moved to OC.Email.Smtp, so remove it on the upcoming major release.
    /// <summary>
    /// Represents an enumeration for mail encryption methods.
    /// </summary>
    public enum SmtpEncryptionMethod
    {
        None = 0,
        SslTls = 1,
        StartTls = 2
    }
}
