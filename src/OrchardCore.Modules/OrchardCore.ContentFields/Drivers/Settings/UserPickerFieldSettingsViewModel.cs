using System;

namespace OrchardCore.ContentFields.Settings
{
    public class UserPickerFieldSettingsViewModel
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public bool Multiple { get; set; }
        public bool DisplayAllUsers { get; set; }
        public RoleEntry[] Roles { get; set; } = Array.Empty<RoleEntry>();
    }

    public class RoleEntry
    {
        public string Role { get; set; }
        public bool IsSelected { get; set; }
    }
}
