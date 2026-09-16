using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Models;

public class SearchFormPart : ContentPart
{
    [Description("The name of the search index queried by the form.")]
    public string IndexName { get; set; }

    [Description("The hint displayed when the search input is empty.")]
    public string Placeholder { get; set; }
}
