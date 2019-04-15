using System;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    public class CalendarSelectorResult
    {
        public int Priority { get; set; }
        public Func<Task<string>> CalendarName { get; set; }
    }
}
