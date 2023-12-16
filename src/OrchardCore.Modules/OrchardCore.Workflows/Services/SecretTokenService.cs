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
        private readonly IClock _clock;

        public SecretTokenService(ISecretProtectionProvider secretProtectionProvider, IClock clock)
        {
            _secretProtectionProvider = secretProtectionProvider;
            _clock = clock;
        }

        public async Task<string> CreateTokenAsync<T>(T payload, TimeSpan lifetime)
        {
            var json = JsonConvert.SerializeObject(payload);

            var protector = await _secretProtectionProvider.CreateProtectorAsync(Secrets.Encryption, Secrets.Signing);

            return await protector.ProtectAsync(json, _clock.UtcNow.Add(lifetime));
        }

        public async Task<(bool, T)> TryDecryptTokenAsync<T>(string token)
        {
            var payload = default(T);

            try
            {
                var unprotector = await _secretProtectionProvider.CreateUnprotectorAsync(token);

                var (Plaintext, Expiration) = await unprotector.UnprotectAsync();
                if (_clock.UtcNow < Expiration.ToUniversalTime())
                {
                    payload = JsonConvert.DeserializeObject<T>(Plaintext);
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
