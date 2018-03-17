using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.Modules;

namespace OrchardCore.Workflows.Services
{
    public class SecurityTokenService : ISecurityTokenService
    {
        private readonly IDataProtector _dataProtector;
        private readonly IClock _clock;

        public SecurityTokenService(
            IDataProtectionProvider dataProtectionProvider, 
            IClock clock,
            IStringLocalizer<SecurityTokenService> localizer)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("Tokens");
            _clock = clock;
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public string CreateToken<T>(T payload, TimeSpan lifetime)
        {
            var expiringPayload = new ExpiringPayload<T> { Payload = payload, ExpireUtc = _clock.UtcNow.Add(lifetime) };

            var json = JsonConvert.SerializeObject(expiringPayload);
            var token = _dataProtector.Protect(json);
            return token;
        }

        public bool TryDecryptToken<T>(string token, out T payload)
        {
            payload = default;

            try
            {
                var json = _dataProtector.Unprotect(token);
                var expiringPayload = JsonConvert.DeserializeObject<ExpiringPayload<T>>(json);

                if (_clock.UtcNow < expiringPayload.ExpireUtc)
                {
                    payload = expiringPayload.Payload;

                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private class ExpiringPayload<T>
        {
            public T Payload { get; set; }
            public DateTime ExpireUtc { get; set; }
        }
    }
}