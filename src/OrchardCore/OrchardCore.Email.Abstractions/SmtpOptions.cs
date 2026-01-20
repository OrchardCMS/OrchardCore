namespace OrchardCore.Email;

public class SmtpOptions
{
    public bool IsEnabled { get; set; }

    public string DefaultSender { get; set; }

    public string Host { get; set; }

    public int Port { get; set; } = 25;

    public bool AutoSelectEncryption { get; set; }

    public bool RequireCredentials { get; set; }

    public bool UseDefaultCredentials { get; set; }

    public SmtpEncryptionMethod EncryptionMethod { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

    public string ProxyHost { get; set; }

    public int ProxyPort { get; set; }

    public bool IgnoreInvalidSslCertificate { get; set; }

    public SmtpDeliveryMethod DeliveryMethod { get; set; }

    public string PickupDirectoryLocation { get; set; }

    public bool ConfigurationExists()
    {
        if (string.IsNullOrEmpty(DefaultSender))
        {
            return false;
        }

        return DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory
            || (DeliveryMethod == SmtpDeliveryMethod.Network && !string.IsNullOrEmpty(Host));
    }
}
