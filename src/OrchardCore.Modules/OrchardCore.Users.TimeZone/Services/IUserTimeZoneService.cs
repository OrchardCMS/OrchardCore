using System.Threading.Tasks;
using OrchardCore.Modules;
using OrchardCore.Users.TimeZone.Models;

namespace OrchardCore.Users.TimeZone.Services
{
    public interface IUserTimeZoneService
    {
        Task<ITimeZone> GetUserTimeZoneAsync();
        Task UpdateUserTimeZoneAsync(UserProfile profile);
        Task<string> GetCurrentUserTimeZoneIdAsync();
    }
}
