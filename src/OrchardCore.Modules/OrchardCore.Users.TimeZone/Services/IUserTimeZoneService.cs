using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.Services
{
    public interface IUserTimeZoneService
    {
        Task<ITimeZone> GetUserTimeZoneAsync();
        Task UpdateUserTimeZoneAsync(string timeZoneId);
        Task<string> GetCurrentUserTimeZoneIdAsync();
    }
}
