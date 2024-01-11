namespace OrchardCore.Email
{
    // TODO: The enum has to be moved to OC.Email.Smtp, so remove it on the upcoming major release.
    /// <summary>
    /// Represents an enumeration for the mail delivery methods.
    /// </summary>
    public enum SmtpDeliveryMethod
    {
        Network,
        SpecifiedPickupDirectory
    }
}
