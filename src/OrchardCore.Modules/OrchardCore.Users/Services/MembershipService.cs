using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public class MembershipService : IMembershipService
{
    private readonly UserManager<IUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<IUser> _claimsPrincipalFactory;
    private readonly PasswordTimingNormalizationService _timingNormalization;

    public MembershipService(
        IUserClaimsPrincipalFactory<IUser> claimsPrincipalFactory,
        UserManager<IUser> userManager,
        PasswordTimingNormalizationService timingNormalization)
    {
        _claimsPrincipalFactory = claimsPrincipalFactory;
        _userManager = userManager;
        _timingNormalization = timingNormalization;
    }

    public async Task<bool> CheckPasswordAsync(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user == null)
        {
            // Perform a dummy hash verification so the response time is
            // indistinguishable from a real password check, preventing
            // username enumeration via timing analysis.
            _timingNormalization.NormalizeResponseTime();

            return false;
        }

        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IUser> GetUserAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);

        return user;
    }

    public Task<ClaimsPrincipal> CreateClaimsPrincipal(IUser user)
    {
        return _claimsPrincipalFactory.CreateAsync(user as User);
    }
}
