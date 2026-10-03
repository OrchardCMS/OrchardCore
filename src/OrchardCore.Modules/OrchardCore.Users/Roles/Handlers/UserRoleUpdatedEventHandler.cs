using Microsoft.AspNetCore.Identity;
using OrchardCore.Security;

namespace OrchardCore.Users.Services;

internal sealed class UserRoleUpdatedEventHandler : IRoleUpdatedEventHandler
{
    private readonly UserManager<IUser> _userManager;

    public UserRoleUpdatedEventHandler(UserManager<IUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task RoleUpdatedAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);

        foreach (var user in users)
        {
            // Update the security stamp to invalidate any existing sessions for the user.
            await _userManager.UpdateSecurityStampAsync(user);
        }
    }
}
