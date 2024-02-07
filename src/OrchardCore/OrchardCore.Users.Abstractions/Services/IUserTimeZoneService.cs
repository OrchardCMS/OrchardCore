using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.Users.Services;

public interface IUserTimeZoneService
{
    Task<ITimeZone> GetUserTimeZoneAsync();

    Task UpdateUserTimeZoneAsync(IUser user);

    Task<string> GetCurrentUserTimeZoneIdAsync();
}
