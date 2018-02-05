using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace OrchardCore.Workflows.Services
{
    public class SecurityTokenService : ISecurityTokenService
    {
        private readonly Lazy<IDataProtector> _dataProtector;

        public SecurityTokenService(IDataProtectionProvider dataProtectionProvider, IStringLocalizer<SecurityTokenService> localizer)
        {
            _dataProtector = new Lazy<IDataProtector>(() => dataProtectionProvider.CreateProtector("OrchardCore.Workflows.Services.SecurityTokenService.V1"));
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public string CreateToken<T>(T payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var token = _dataProtector.Value.Protect(json);
            return token;
        }

        public Result<T> DecryptToken<T>(string token)
        {
            try
            {
                var json = _dataProtector.Value.Unprotect(token);
                var payload = JsonConvert.DeserializeObject<T>(json);

                return Result.Ok(payload);
            }
            catch (Exception ex)
            {
                return Result.Fail<T>(this.T[ex.Message]);
            }
        }
    }
}