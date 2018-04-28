using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Modules
{
    public class TimeZone : ITimeZone
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Comment { get; set; }

        public TimeZone(string id, string displayName, string comment)
        {
            Id = id;
            DisplayName = displayName;
            Comment = comment;
        }
    }
}
