using System.ComponentModel;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentFields.Settings;

public class UserPickerFieldSettings : FieldSettings
{
    public bool Multiple { get; set; }

    [DefaultValue(true)]
    public bool DisplayAllUsers { get; set; } = true;

    public string[] DisplayedRoles { get; set; } = [];
}
