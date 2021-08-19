using System;

namespace OrchardCore.ContentFields.Settings
{
    public class TimeFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public string Step { get; set; } = String.Empty;
    }
}
