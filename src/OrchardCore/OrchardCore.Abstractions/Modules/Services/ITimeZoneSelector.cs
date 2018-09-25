using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Provides the timezone for the current request.
    /// </summary>
    public interface ITimeZoneSelector
    {
        Task<TimeZoneSelectorResult> GetTimeZoneAsync();
    }
}
