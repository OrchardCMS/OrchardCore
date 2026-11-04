using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class UserPickerField : ContentField
{
    [Description("The IDs of the selected users.")]
    public string[] UserIds { get; set; } = [];
}
