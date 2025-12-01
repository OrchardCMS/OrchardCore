using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.ContentFields.Settings.Models;

public class TextFieldSettingsViewModel
{
    public string Hint { get; set; }

    public bool Required { get; set; }

    public string DefaultValue { get; set; }

    public FieldBehaviorType Type { get; set; }

    /// <summary>
    /// The pattern used to build the value.
    /// </summary>
    public string Pattern { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Types { get; set; }
}
