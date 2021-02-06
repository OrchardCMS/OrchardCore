using System;

namespace OrchardCore.ContentFields.Settings
{
    public class UserPickerFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public bool Multiple { get; set; }
        public bool DisplayAllUsers { get; set; }
        public string[] DisplayedRoles { get; set; } = Array.Empty<string>();
    }
}
