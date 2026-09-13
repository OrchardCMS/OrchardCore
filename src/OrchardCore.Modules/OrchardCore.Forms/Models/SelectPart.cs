using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class SelectPart : ContentPart
{
    [Description("The choices available in the select element.")]
    public SelectOption[] Options { get; set; }

    [Description("The value selected initially.")]
    public string DefaultValue { get; set; }

    [Description("The control style used to present the choices.")]
    public SelectEditorOption Editor { get; set; }
}

public class SelectOption
{
    [Description("The text displayed for the option.")]
    public string Text { get; set; }

    [Description("The value submitted when the option is selected.")]
    public string Value { get; set; }
}

public enum SelectEditorOption
{
    Dropdown,
    Checkbox,
    Radio,
}
