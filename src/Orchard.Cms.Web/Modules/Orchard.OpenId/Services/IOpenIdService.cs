using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.OpenId.Settings;

namespace Orchard.OpenId.Services
{
    public interface IOpenIdService
    {
        Task<OpenIdSettings> GetOpenIdSettingsAsync();
        Task UpdateOpenIdSettingsAsync(OpenIdSettings openIdSettings);
        bool IsValidOpenIdSettings(OpenIdSettings openIdSettings);
        bool IsValidOpenIdSettings(OpenIdSettings settings, ModelStateDictionary modelState);
        IEnumerable<CertificateInfo> GetAvailableCertificates(bool onlyCertsWithPrivateKey);
    }
}
