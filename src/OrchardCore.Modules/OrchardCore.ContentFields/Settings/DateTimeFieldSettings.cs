using System;

namespace OrchardCore.ContentFields.Settings
{
    public class DateTimeFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public bool Currently { get; set; } = false;
        public DateTime? DefaultValue { get; set; }
    }
}
