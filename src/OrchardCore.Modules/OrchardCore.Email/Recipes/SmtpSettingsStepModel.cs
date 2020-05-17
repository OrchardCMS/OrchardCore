using OrchardCore.Email;

namespace OrchardCore.Email
{
    public class SmtpSettingsStepModel
    {
        public string DefaultSender { get; set; }
        public SmtpDeliveryMethod DeliveryMethod { get; set; }
        public string PickupDirectoryLocation { get; set; }
        public string Host { get; set; }
        public int Port { get; set; } = 25;
        public bool AutoSelectEncryption { get; set; }
        public bool RequireCredentials { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public SmtpEncryptionMethod EncryptionMethod { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
