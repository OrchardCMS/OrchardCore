using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OrchardCore.Modules;

namespace OrchardCore.Secrets.Services;

public class SecretTokenService : ISecretTokenService
{
    private readonly ISecretProtector _secretProtector;
    private readonly IClock _clock;

    public SecretTokenService(ISecretProtectionProvider secretProtectionProvider, IClock clock)
    {
        _secretProtector = secretProtectionProvider.CreateProtector(TokenSecrets.Purpose);
        _clock = clock;
    }

    public Task<string> CreateTokenAsync<T>(T payload, TimeSpan lifetime)
    {
        var json = JsonConvert.SerializeObject(payload);

        return _secretProtector.ProtectAsync(json, _clock.UtcNow.Add(lifetime));
    }

    public async Task<(bool, T)> TryDecryptTokenAsync<T>(string token)
    {
        var payload = default(T);

        try
        {
            var (Plaintext, Expiration) = await _secretProtector.UnprotectAsync(token);
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
