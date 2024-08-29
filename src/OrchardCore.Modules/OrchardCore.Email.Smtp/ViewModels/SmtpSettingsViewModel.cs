using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Email.Smtp.ViewModels;

public class SmtpSettingsViewModel
{
    public bool IsEnabled { get; set; }

    public string DefaultSender { get; set; }

    public string Host { get; set; }

    [Range(0, 65535)]
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
}
