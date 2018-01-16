using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.Models;

namespace OrchardCore.OpenId.Services
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
