using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.OpenId.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.Services
{
    public interface IOpenIdService
    {
        Task<OpenIdSettings> GetOpenIdSettingsAsync();
        bool IsValidOpenIdSettings(OpenIdSettings openIdSettings);
        bool IsValidOpenIdSettings(OpenIdSettings settings, ModelStateDictionary modelState);
        IEnumerable<CertificateInfo> GetAvailableCertificates();
    }
}
