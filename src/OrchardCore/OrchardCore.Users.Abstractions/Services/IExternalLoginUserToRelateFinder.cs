using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.Abstractions;

public interface IExternalLoginUserToRelateFinder
{
    bool CanHandle(ExternalLoginInfo info);

    Task<IUser> GetUserAsync(ExternalLoginInfo info);

    string GetValueThatLinkAccount(ExternalLoginInfo info);
}
