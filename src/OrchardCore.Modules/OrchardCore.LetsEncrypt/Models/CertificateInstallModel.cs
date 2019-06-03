using Certes;

namespace OrchardCore.LetsEncrypt.Models
{
    public class CertificateInstallModel
    {
        public CsrInfo CertInfo { get; set; }
        public byte[] PfxCertificate { get; set; }
        public string[] Hostnames { get; set; }
    }
}
