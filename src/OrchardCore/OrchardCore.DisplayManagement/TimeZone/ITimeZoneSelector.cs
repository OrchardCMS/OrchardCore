using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.TimeZone
{
    public interface ITimeZoneSelector
    {
        Task<TimeZoneSelectorResult> GetTimeZoneAsync();
    }
}
