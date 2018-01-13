using System;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Workflows.Services
{
    public class SignalService : ISignalService
    {
        private readonly Lazy<IDataProtector> _dataProtector;

        public SignalService(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtector = new Lazy<IDataProtector>(() => dataProtectionProvider.CreateProtector("OrchardCore.Workflows.Services.SignalService.V1"));
        }

        public string CreateToken(string correlationId, string signal)
        {
            var payload = new
            {
                C = correlationId,
                S = signal
            };
            var json = JsonConvert.SerializeObject(payload);
            var token = _dataProtector.Value.Protect(json);
            return token;
        }

        public bool DecryptToken(string token, out string correlationId, out string signal)
        {
            correlationId = null;
            signal = "";

            try
            {
                var json = _dataProtector.Value.Unprotect(token);
                dynamic payload = JObject.Parse(json);
                correlationId = payload.C;
                signal = payload.S;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}