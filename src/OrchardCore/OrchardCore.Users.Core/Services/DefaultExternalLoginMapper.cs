using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.Services;

public class DefaultExternalLoginMapper : IExternalLoginMapper
{
    private readonly UserManager<IUser> _userManager;

    public DefaultExternalLoginMapper(UserManager<IUser> userManager)
    {
        _userManager = userManager;
    }

    public bool CanHandle(ExternalLoginInfo info)
        => true;

    public async Task<IUser> GetUserAsync(ExternalLoginInfo info)
    {
        var email = info.GetEmail();

        return !string.IsNullOrWhiteSpace(email) ? await _userManager.FindByEmailAsync(email) : null;
    }
}
