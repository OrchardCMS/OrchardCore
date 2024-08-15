using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.Services;

public class EmailClaimsProvider : IUserClaimsProvider
{
    private readonly UserManager<IUser> _userManager;

    public EmailClaimsProvider(UserManager<IUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GenerateAsync(IUser user, ClaimsIdentity claims)
    {
        ArgumentNullException.ThrowIfNull(user);

        ArgumentNullException.ThrowIfNull(claims);

        // Todo: In a future version the base implementation will generate the email claim if the user store is an 'IUserEmailStore',
        // so we will not have to add it here, and everywhere we are using the hardcoded "email" claim type, we will have to use the
        // new 'IdentityOptions.ClaimsIdentity.EmailClaimType' or at least its default value which is 'ClaimTypes.Email'.

        var email = await _userManager.GetEmailAsync(user);
        if (!string.IsNullOrEmpty(email))
        {
            claims.AddClaim(new Claim("email", email));

            var confirmed = await _userManager.IsEmailConfirmedAsync(user);
            claims.AddClaim(new Claim("email_verified", confirmed ? bool.TrueString : bool.FalseString, ClaimValueTypes.Boolean));
        }
    }
}
