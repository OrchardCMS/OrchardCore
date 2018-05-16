using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.Settings.Services
{
    public interface IDefaultTimeZoneService
    {
        Task<ITimeZone> GetSiteTimeZoneAsync();
        Task SetSiteTimeZoneAsync(string timeZoneId);
        Task<string> GetCurrentTimeZoneIdAsync();
    }
}
