using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.OpenIdConnect.Services
{
    public interface IOpenIdConnectService
    {
        Task<OpenIdConnectSettings> GetOpenIdConnectSettings();
        Task UpdateOpenIdConnectSettingsAsync(OpenIdConnectSettings settings);
        bool IsValidOpenIdConnectSettings(OpenIdConnectSettings settings, ModelStateDictionary modelState);
        bool IsValidOpenIdConnectSettings(OpenIdConnectSettings settings);
        string Protect(string value);
        string Unprotect(string value);
        
    }
}
