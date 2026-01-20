using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace OrchardCore.Users.Services;

public class ConfigureSecurityStampOptions : IPostConfigureOptions<SecurityStampValidatorOptions>
{
    public void PostConfigure(string name, SecurityStampValidatorOptions options)
    {
        // While updating the principal, any customized claims set using IUserClaimsProvider
        // or obtained from an external identity will persist and transfer to the refreshed/new principal.
        options.OnRefreshingPrincipal = principalContext =>
        {
            var currentIdentity = principalContext.CurrentPrincipal?.Identities?.FirstOrDefault();

            if (currentIdentity is not null && principalContext.NewPrincipal.Identities is not null)
            {
                var newIdentity = principalContext.NewPrincipal.Identities.First();

                foreach (var claim in currentIdentity.Claims)
                {
                    if (newIdentity.HasClaim(claim.Type, claim.Value))
                    {
                        continue;
                    }

                    newIdentity.AddClaim(new Claim(claim.Type, claim.Value));
                }
            }

            return Task.CompletedTask;
        };
    }
}
