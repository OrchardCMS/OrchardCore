using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.TimeZone
{
    /// <summary>
    /// Provides the timezone for the current request.
    /// </summary>
    public interface ITimeZoneSelector
    {
        Task<TimeZoneSelectorResult> GetTimeZoneAsync();
    }
}
