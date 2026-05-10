using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.OpenSearch.Core.Models;

public class ContentPickerFieldOpenSearchEditorSettings
{
    public string Index { get; set; }

    [BindNever]
    public string[] Indices { get; set; }
}
