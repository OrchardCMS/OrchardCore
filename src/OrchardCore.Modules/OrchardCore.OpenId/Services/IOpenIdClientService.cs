using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.OpenId.Settings;
using System.Threading.Tasks;

namespace OrchardCore.OpenId.Services
{
    public interface IOpenIdClientService
    {
        Task<OpenIdClientSettings> GetOpenIdConnectSettings();
        Task UpdateOpenIdConnectSettingsAsync(OpenIdClientSettings settings);
        bool IsValidOpenIdConnectSettings(OpenIdClientSettings settings, ModelStateDictionary modelState);
        bool IsValidOpenIdConnectSettings(OpenIdClientSettings settings);
        string Protect(string value);
        string Unprotect(string value);
    }
}
