using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Abstractions;

namespace OrchardCore.Users.Services;

public class DefaultExternalLoginUserToRelateFinder : IExternalLoginUserToRelateFinder
{
    private readonly UserManager<IUser> _userManager;

    public DefaultExternalLoginUserToRelateFinder(UserManager<IUser> userManager)
    {
        _userManager = userManager;
    }

    public bool CanHandle(ExternalLoginInfo info)
    {
        return true;
    }

    public async Task<IUser> GetUserAsync(ExternalLoginInfo info)
    {
        // the default behavior previously used in OrchardCore
        var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");

        IUser iUser = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            iUser = await _userManager.FindByEmailAsync(email);
        }

        return iUser;
    }

    public string GetValueThatLinkAccount(ExternalLoginInfo info)
    {
        return info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");
    }
}
