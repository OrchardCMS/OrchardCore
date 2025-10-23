namespace OrchardCore.Catalogs.Models;

public class ListSourceCatalogEntryViewModel<T> : ListSourceModelViewModel
{
    public IList<CatalogEntryViewModel<T>> Models { get; set; }
}
