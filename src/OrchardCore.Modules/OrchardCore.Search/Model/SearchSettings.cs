namespace OrchardCore.Search.Models;

public class SearchSettings
{
    public string DefaultIndexId { get; set; }

    [Obsolete("This property is no longer used. Instead use DefaultIndexId property to set the IndexEntityId.")]
    public string ProviderName { get; set; }

    public string Placeholder { get; set; }

    public string PageTitle { get; set; }
}
