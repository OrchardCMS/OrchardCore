using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using OrchardCore.Users;
using OrchardCore.Users.Handlers;

namespace OrchardCore.OpenId.Handlers;

public sealed class OpenIdExternalLoginEventHandler : IExternalLoginEventHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OpenIdExternalLoginEventHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<string> GenerateUserName(string provider, IEnumerable<SerializableClaim> claims)
        => Task.FromResult<string>(null);

    public Task UpdateUserAsync(UpdateUserContext context)
        => Task.CompletedTask;

    public async Task SetLogoutAsync(string provider, Dictionary<string, string> properties)
    {
        if (provider != "OpenIdConnect")
        {
            return;
        }

        var result = await _httpContextAccessor.HttpContext.AuthenticateAsync();

        if (result is not { Principal.Identity: ClaimsIdentity _ })
        {
            return;
        }

        // Some how initiate the logout request at the server.

        properties[".identity_token_hint"] = result.Properties.GetTokenValue("backchannel_id_token");
        properties[".registration_id"] = provider;
    }
}
