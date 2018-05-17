using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.Services
{
    public interface IUserTimeZoneService
    {
        Task<ITimeZone> GetSiteTimeZoneAsync();
        Task SetSiteTimeZoneAsync(string timeZoneId);
        Task<string> GetCurrentTimeZoneIdAsync();
    }
}
