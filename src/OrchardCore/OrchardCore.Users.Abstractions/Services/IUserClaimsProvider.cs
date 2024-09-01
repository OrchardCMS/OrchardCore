using System.Security.Claims;

namespace OrchardCore.Users.Services;

public interface IUserClaimsProvider
{
    Task GenerateAsync(IUser user, ClaimsIdentity claims);
}
