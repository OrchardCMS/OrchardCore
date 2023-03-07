using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Model;

public class SearchFormPart : ContentPart
{
    public string IndexName { get; set; }

    public string Placeholder { get; set; }
}
