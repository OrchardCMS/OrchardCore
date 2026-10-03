using Microsoft.AspNetCore.Identity;
using OrchardCore.Security;

namespace OrchardCore.Users.Services;

public class UserRoleRemovedEventHandler : IRoleRemovedEventHandler
{
    private readonly UserManager<IUser> _userManager;

    public UserRoleRemovedEventHandler(UserManager<IUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task RoleRemovedAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);

        foreach (var user in users)
        {
            await _userManager.RemoveFromRoleAsync(user, roleName);

            // Update the security stamp to invalidate any existing sessions for the user.
            await _userManager.UpdateSecurityStampAsync(user);
        }
    }
}
