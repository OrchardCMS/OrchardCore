using System.Threading.Tasks;
using OrchardCore.LetsEncrypt.Models;

namespace OrchardCore.LetsEncrypt.Services
{
    public interface ILetsEncryptService
    {
        Task<CertificateInstallModel> RequestHttpChallengeCertificate(string registrationEmail, string[] hostnames, bool useStaging);
        string GetChallengeKeyFilename(string token);
    }
}
