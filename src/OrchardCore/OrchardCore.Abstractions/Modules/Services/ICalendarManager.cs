using System.Collections.Generic;
using System.Globalization;

namespace OrchardCore.Modules
{
    public interface ICalendarManager
    {
        IEnumerable<string> GetCalendars();
        Calendar GetCalendarByName(string calendarName);
    }
}