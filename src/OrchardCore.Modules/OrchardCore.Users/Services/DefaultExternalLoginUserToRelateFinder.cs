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

    public string GetValueThatLinkAccount(ExternalLoginInfo info)
        => info.GetEmail();
}
