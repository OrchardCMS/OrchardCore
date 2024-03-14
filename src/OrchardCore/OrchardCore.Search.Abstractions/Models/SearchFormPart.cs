using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Models;

public class SearchFormPart : ContentPart
{
    public string IndexName { get; set; }

    public string Placeholder { get; set; }
}
