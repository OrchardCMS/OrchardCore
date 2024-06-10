using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.Services;

public class DefaultUserToExternalLoginProvider : IUserToExternalLoginProvider
{
    private readonly UserManager<IUser> _userManager;

    public DefaultUserToExternalLoginProvider(UserManager<IUser> userManager)
    {
        _userManager = userManager;
    }

    public bool CanHandle(ExternalLoginInfo info)
        => true;

    public async Task<IUser> GetUserAsync(ExternalLoginInfo info)
    {
        var email = info.GetEmail();

        if (!string.IsNullOrWhiteSpace(email))
        {
            return await _userManager.FindByEmailAsync(email);
        }

        return null;
    }

    public string GetIdentifierKey(ExternalLoginInfo info)
        => info.GetEmail();
}
