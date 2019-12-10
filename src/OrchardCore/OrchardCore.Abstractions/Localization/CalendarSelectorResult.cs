using System;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a result for the selected calendar.
    /// </summary>
    public class CalendarSelectorResult
    {
        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets a calendar name.
        /// </summary>
        public Func<Task<CalendarName>> CalendarName { get; set; }
    }
}
