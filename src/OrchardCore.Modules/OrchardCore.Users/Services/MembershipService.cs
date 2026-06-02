using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public class MembershipService : IMembershipService
{
    private readonly IPasswordAuthenticationTimingService _passwordAuthenticationTimingService;
    private readonly UserManager<IUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<IUser> _claimsPrincipalFactory;

    public MembershipService(
        IPasswordAuthenticationTimingService passwordAuthenticationTimingService,
        IUserClaimsPrincipalFactory<IUser> claimsPrincipalFactory,
        UserManager<IUser> userManager)
    {
        _passwordAuthenticationTimingService = passwordAuthenticationTimingService;
        _claimsPrincipalFactory = claimsPrincipalFactory;
        _userManager = userManager;
    }

    public async Task<bool> CheckPasswordAsync(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user == null)
        {
            await _passwordAuthenticationTimingService.MitigateUnknownUserAsync(password);
            return false;
        }

        var result = await _userManager.CheckPasswordAsync(user, password);

        if (!result)
        {
            await _passwordAuthenticationTimingService.DelayFailedAuthenticationAsync();
        }

        return result;
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
