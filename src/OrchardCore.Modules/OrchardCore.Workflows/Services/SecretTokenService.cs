using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OrchardCore.Modules;
using OrchardCore.Secrets;

namespace OrchardCore.Workflows.Services
{
    public class SecretTokenService : ISecretTokenService
    {
        private readonly ISecretProtectionProvider _secretProtectionProvider;
        // private readonly IClock _clock;

        public SecretTokenService(ISecretProtectionProvider secretProtectionProvider, IClock clock)
        {
            _secretProtectionProvider = secretProtectionProvider;
            // _clock = clock;
        }

        public async Task<string> CreateTokenAsync<T>(T payload, TimeSpan lifetime)
        {
            var json = JsonConvert.SerializeObject(payload);

            // Todo: Time limited encryptions using the provided lifetime.
            var encryptor = await _secretProtectionProvider.CreateEncryptorAsync(Secrets.Encryption, Secrets.Signing);

            return encryptor.Encrypt(json);
        }

        public async Task<(bool, T)> TryDecryptTokenAsync<T>(string token)
        {
            var payload = default(T);

            try
            {
                // Todo: Make time limited decryptions using the provided lifetime.
                var protector = await _secretProtectionProvider.CreateDecryptorAsync(token);


                // Todo: Time limited decryptions providing an expiration.
                var json = protector.Decrypt();

                // if (_clock.UtcNow < expiration.ToUniversalTime())
                {
                    payload = JsonConvert.DeserializeObject<T>(json);
                    return (true, payload);
                }
            }
            catch
            {
            }

            return (false, payload);
        }
    }
}
