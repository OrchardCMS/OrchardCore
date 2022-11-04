using System;
using System.ComponentModel;

namespace OrchardCore.ContentFields.Settings
{
    public class UserPickerFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public bool Multiple { get; set; }

        [DefaultValue(true)]
        public bool DisplayAllUsers { get; set; } = true;
        public string[] DisplayedRoles { get; set; } = Array.Empty<string>();
    }
}
