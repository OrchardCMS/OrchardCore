using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Model;

public class SearchPart : ContentPart
{
    public string IndexName { get; set; }

    public string Placeholder { get; set; }
}
