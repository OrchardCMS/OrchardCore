using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.OpenId.Models;
using System.Security.Claims;

namespace Orchard.OpenId.Services
{
    public interface IOpenIdApplicationManager
    {
        Task<string> CreateAsync(OpenIdApplication application);
        Task<OpenIdApplication> FindByClientIdAsync(string identifier);
        Task<OpenIdApplication> FindByIdAsync(string identifier);
        Task<OpenIdApplication> FindByLogoutRedirectUri(string url);
        Task<string> GetClientTypeAsync(OpenIdApplication application);
        Task<string> GetDisplayNameAsync(OpenIdApplication application);
        Task<IEnumerable<string>> GetTokensAsync(OpenIdApplication application);
        Task<bool> ValidateRedirectUriAsync(OpenIdApplication application, string address);
        Task<bool> ValidateSecretAsync(OpenIdApplication application, string secret);
        Task<IEnumerable<OpenIdApplication>> GetAppsAsync(int skip, int pageSize);
        Task<int> GetCount();
        Task<IList<string>> GetRolesAsync(OpenIdApplication application);
        Task<ClaimsIdentity> CreateIdentityAsync(OpenIdApplication application, IEnumerable<string> scopes);
        Task DeleteAsync(OpenIdApplication application);
    }
}