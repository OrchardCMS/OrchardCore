using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Users.Services;

/// <summary>
/// Custom implementation of <see cref="IUserClaimsPrincipalFactory{TUser}"/> allowing adding claims by implementing the <see cref="IUserClaimsProvider"/>.
/// </summary>
public class DefaultUserClaimsPrincipalProviderFactory : UserClaimsPrincipalFactory<IUser>
{
    private readonly IEnumerable<IUserClaimsProvider> _claimsProviders;
    private readonly ILogger _logger;

    public DefaultUserClaimsPrincipalProviderFactory(
        UserManager<IUser> userManager,
        IOptions<IdentityOptions> identityOptions,
        IEnumerable<IUserClaimsProvider> claimsProviders,
        ILogger<DefaultUserClaimsPrincipalProviderFactory> logger)
        : base(userManager, identityOptions)
    {
        _claimsProviders = claimsProviders;
        _logger = logger;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IUser user)
    {
        var claims = await base.GenerateClaimsAsync(user);

        await _claimsProviders.InvokeAsync((claimsProvider) => claimsProvider.GenerateAsync(user, claims), _logger);

        return claims;
    }
}
