using System;
using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    public class CalendarSelectorResult
    {
        public int Priority { get; set; }
        public Func<Task<string>> CalendarName { get; set; }
    }
}
