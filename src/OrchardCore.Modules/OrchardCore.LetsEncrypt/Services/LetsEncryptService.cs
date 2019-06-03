using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.LetsEncrypt.Models;

namespace OrchardCore.LetsEncrypt.Services
{
    public class LetsEncryptService : ILetsEncryptService
    {
        private readonly IOptions<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public LetsEncryptService(
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings,
            ILogger<LetsEncryptService> logger
            )
        {
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public async Task<CertificateInstallModel> RequestHttpChallengeCertificate(string registrationEmail, string[] hostnames, bool useStaging)
        {
            var acmeContext = await GetOrCreateAcmeContext(registrationEmail, useStaging);

            var order = await acmeContext.NewOrder(hostnames);

            // TODO: Handle authorizations in another method
            foreach (var authorizationContext in await order.Authorizations())
            {
                var httpChallenge = await authorizationContext.Http();
                var keyAuthorizationString = httpChallenge.KeyAuthz;

                var keyAuthorizationFilename = GetChallengeKeyFilename(httpChallenge.Token);

                Directory.CreateDirectory(Path.GetDirectoryName(keyAuthorizationFilename));
                File.WriteAllText(keyAuthorizationFilename, keyAuthorizationString);

                // Split to a separate function
                var challengeResponse = await httpChallenge.Validate();

                while (challengeResponse.Status == Certes.Acme.Resource.ChallengeStatus.Pending || challengeResponse.Status == Certes.Acme.Resource.ChallengeStatus.Processing)
                {
                    await Task.Delay(5000);
                    challengeResponse = await httpChallenge.Resource();
                }

                if (challengeResponse.Status == Certes.Acme.Resource.ChallengeStatus.Invalid)
                {
                    throw new Exception($"Let's Encrypt challenge failed. {challengeResponse.Error?.Detail}");
                }
            }

            var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);

            var certInfo = new CsrInfo
            {
                CommonName = hostnames[0]
            };

            // TODO: Split out into a separate function
            var cert = await order.Generate(certInfo, privateKey);

            var certPem = cert.ToPem();

            var pfxBuilder = cert.ToPfx(privateKey);

            var pfx = pfxBuilder.Build(hostnames[0], string.Empty);

            return new CertificateInstallModel
            {
                CertInfo = certInfo,
                PfxCertificate = pfx,
                Hostnames = hostnames
            };
        }

        public string GetChallengeKeyFilename(string token)
        {
            return PathExtensions.Combine(
                _shellOptions.Value.ShellsApplicationDataPath,
                _shellOptions.Value.ShellsContainerName,
                _shellSettings.Name,
                "Lets-Encrypt",
                "Challenge-Keys",
                token
            );
        }

        private async Task<AcmeContext> GetOrCreateAcmeContext(string registrationEmail, bool useStaging)
        {
            AcmeContext acmeContext;

            var letsEncryptUri = useStaging ? WellKnownServers.LetsEncryptStagingV2 : WellKnownServers.LetsEncryptV2;

            var pemKeyFilename = PathExtensions.Combine(
                _shellOptions.Value.ShellsApplicationDataPath,
                _shellOptions.Value.ShellsContainerName,
                _shellSettings.Name,
                "Lets-Encrypt",
                "Account-Keys",
                $"{registrationEmail}-{letsEncryptUri.Host}.pem");

            if (!File.Exists(pemKeyFilename))
            {
                acmeContext = new AcmeContext(letsEncryptUri);
                var account = await acmeContext.NewAccount(registrationEmail, true);

                var pemKey = acmeContext.AccountKey.ToPem();

                Directory.CreateDirectory(Path.GetDirectoryName(pemKeyFilename));
                File.WriteAllText(pemKeyFilename, pemKey);

                acmeContext = new AcmeContext(letsEncryptUri, acmeContext.AccountKey, new AcmeHttpClient(letsEncryptUri, new HttpClient()));
            }
            else
            {
                var pemKey = File.ReadAllText(pemKeyFilename);
                var accountKey = KeyFactory.FromPem(pemKey);

                acmeContext = new AcmeContext(letsEncryptUri, accountKey, new AcmeHttpClient(letsEncryptUri, new HttpClient()));
            }

            return acmeContext;
        }
    }
}
