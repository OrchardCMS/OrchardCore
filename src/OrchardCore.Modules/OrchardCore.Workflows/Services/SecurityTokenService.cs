using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using OrchardCore.Modules;

namespace OrchardCore.Workflows.Services;

public class SecurityTokenService : ISecurityTokenService
{
    private readonly ITimeLimitedDataProtector _dataProtector;
    private readonly IClock _clock;

    public SecurityTokenService(
        IDataProtectionProvider dataProtectionProvider,
        IClock clock)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("Tokens").ToTimeLimitedDataProtector();
        _clock = clock;
    }

    public string CreateToken<T>(T payload, TimeSpan lifetime)
    {
        var json = JConvert.SerializeObject(payload);

        return _dataProtector.Protect(json, _clock.UtcNow.Add(lifetime));
    }

    public bool TryDecryptToken<T>(string token, out T payload)
    {
        payload = default;

        try
        {
            var json = _dataProtector.Unprotect(token, out var expiration);

            if (_clock.UtcNow < expiration.ToUniversalTime())
            {
                payload = JConvert.DeserializeObject<T>(json);
                return true;
            }
        }
        catch
        {
        }

        return false;
    }
}
